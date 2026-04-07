using System.Collections.Concurrent;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using LinkPara.Card.Application.Commons.Constants;
using LinkPara.Card.Application.Commons.Enums;
using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.FileIngestion;
using LinkPara.Card.Application.Commons.Models.Logging;
using LinkPara.Card.Domain.Constants;
using LinkPara.Card.Domain.Entities.FileIngestion;
using LinkPara.Card.Domain.Enums;
using LinkPara.Card.Infrastructure.Persistence;
using LinkPara.Card.Infrastructure.Services.FileIngestion.RemoteFiles;
using LinkPara.ContextProvider;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LinkPara.Card.Infrastructure.Services.FileIngestion.Orchestration;

public partial class FileIngestionService : IFileIngestionService
{
    private readonly FileIngestionSettings _settings;
    private readonly IRemoteFileReader _remoteFileFetcher;
    private readonly CardDbContext _dbContext;
    private readonly IReadOnlyCollection<IFileParser> _parsers;
    private readonly IReconciliationAlarmService _alarmService;
    private readonly IReconciliationService _reconciliationService;
    private readonly IReconciliationAutoOperationService _reconciliationAutoOperationService;
    private readonly IRemoteFileWriter _archiveRemoteFileWriter;
    private readonly ILogger<FileIngestionService> _logger;
    private readonly IContextProvider _contextProvider;
    private readonly IBulkServiceOperationLogPublisher _bulkLogPublisher;

    public FileIngestionService(
        IOptions<FileIngestionSettings> options,
        IRemoteFileReader remoteFileFetcher,
        CardDbContext dbContext,
        IEnumerable<IFileParser> parsers,
        IReconciliationAlarmService alarmService,
        IReconciliationService reconciliationService,
        IReconciliationAutoOperationService reconciliationAutoOperationService,
        IRemoteFileWriter archiveRemoteFileWriter,
        ILogger<FileIngestionService> logger,
        IContextProvider contextProvider,
        IBulkServiceOperationLogPublisher bulkLogPublisher)
    {
        _settings = options.Value;
        _remoteFileFetcher = remoteFileFetcher;
        _dbContext = dbContext;
        _parsers = parsers.ToArray();
        _alarmService = alarmService;
        _reconciliationService = reconciliationService;
        _reconciliationAutoOperationService = reconciliationAutoOperationService;
        _archiveRemoteFileWriter = archiveRemoteFileWriter;
        _logger = logger;
        _contextProvider = contextProvider;
        _bulkLogPublisher = bulkLogPublisher;
    }

    public Task<IReadOnlyCollection<FileIngestionResult>> ImportCardTransactionsFromLocalDirectoryAsync(
        string directoryPath,
        bool resetExisting = false,
        FileIngestionResetScope resetScope = FileIngestionResetScope.Hash,
        CancellationToken cancellationToken = default)
    {
        var incomingPath = ResolveLocalIncomingPath(
            directoryPath,
            ResolveConfiguredOrOsDefaultPath(
                _settings.Local.CardTransactionsPath,
                defaults => defaults.CardTransactionsPath,
                _settings.Local.Defaults),
            _settings.Local.DefaultDriveCode);

        return RunWithBulkLogAsync(
            nameof(ImportCardTransactionsFromLocalDirectoryAsync),
            async () => await ImportFromLocalAsync(
                incomingPath,
                resetExisting,
                resetScope,
                x => IsCardFileName(x),
                cancellationToken),
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["ResetExisting"] = resetExisting.ToString(),
                ["ResetScope"] = resetScope.ToString(),
                ["IncomingPath"] = incomingPath ?? string.Empty
            },
            cancellationToken);
    }

    public Task<IReadOnlyCollection<FileIngestionResult>> ImportClearingFromLocalDirectoryAsync(
        string directoryPath,
        bool resetExisting = false,
        FileIngestionResetScope resetScope = FileIngestionResetScope.Hash,
        CancellationToken cancellationToken = default)
    {
        var incomingSources = ResolveLocalClearingIncomingPaths(directoryPath, _settings.Local);

        return RunWithBulkLogAsync(
            nameof(ImportClearingFromLocalDirectoryAsync),
            async () =>
            {
                var allResults = new List<FileIngestionResult>();
                var remaining = _settings.MaxFilesPerRun > 0 ? _settings.MaxFilesPerRun : int.MaxValue;

                foreach (var incomingPath in incomingSources)
                {
                    if (remaining <= 0)
                    {
                        break;
                    }

                    var results = await ImportFromLocalAsync(
                        incomingPath,
                        resetExisting,
                        resetScope,
                        x => IsClearingFileName(x),
                        cancellationToken,
                        enforceMaxFilesPerRun: false,
                        maxFilesLimit: remaining);

                    allResults.AddRange(results);
                    remaining -= results.Count;
                }

                return allResults;
            },
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["ResetExisting"] = resetExisting.ToString(),
                ["ResetScope"] = resetScope.ToString(),
                ["IncomingPath"] = string.Join(",", incomingSources)
            },
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<FileIngestionResult>> ImportCardTransactionsFromFtpAsync(
        bool resetExisting = false,
        FileIngestionResetScope resetScope = FileIngestionResetScope.Hash,
        CancellationToken cancellationToken = default)
    {
        return await RunWithBulkLogAsync(
            nameof(ImportCardTransactionsFromFtpAsync),
            async () =>
            {
                var cardIncomingPath = _settings.Ftp.Source.CardTransactionsPath;
                var fileNames = await _remoteFileFetcher.ListFilesAsync(cardIncomingPath, cancellationToken);
                var filtered = fileNames.Where(IsCardFileName).ToArray();
                return await ImportFromFtpFileNamesAsync(
                    filtered,
                    cardIncomingPath,
                    resetExisting,
                    resetScope,
                    cancellationToken);
            },
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["ResetExisting"] = resetExisting.ToString(),
                ["ResetScope"] = resetScope.ToString(),
                ["IncomingPath"] = _settings.Ftp.Source.CardTransactionsPath ?? string.Empty
            },
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<FileIngestionResult>> ImportClearingFromFtpAsync(
        bool resetExisting = false,
        FileIngestionResetScope resetScope = FileIngestionResetScope.Hash,
        CancellationToken cancellationToken = default)
    {
        var sources = ResolveClearingFtpSources();
        return await RunWithBulkLogAsync(
            nameof(ImportClearingFromFtpAsync),
            async () =>
            {
                var allResults = new List<FileIngestionResult>();
                var remaining = _settings.MaxFilesPerRun > 0 ? _settings.MaxFilesPerRun : int.MaxValue;

                foreach (var source in sources)
                {
                    if (remaining <= 0)
                    {
                        break;
                    }

                    var fileNames = await _remoteFileFetcher.ListFilesAsync(source.IncomingPath, cancellationToken);
                    var filtered = fileNames.Where(IsClearingFileName).OrderBy(x => x, StringComparer.OrdinalIgnoreCase);
                    var selected = filtered.Take(remaining).ToArray();
                    if (selected.Length == 0)
                    {
                        continue;
                    }

                    var results = await ImportFromFtpFileNamesAsync(
                        selected,
                        source.IncomingPath,
                        resetExisting,
                        resetScope,
                        cancellationToken,
                        enforceMaxFilesPerRun: false);

                    allResults.AddRange(results);
                    remaining -= selected.Length;
                }

                return allResults;
            },
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["ResetExisting"] = resetExisting.ToString(),
                ["ResetScope"] = resetScope.ToString(),
                ["IncomingPath"] = string.Join(",", sources.Select(x => x.IncomingPath))
            },
            cancellationToken);
    }

    private async Task<IReadOnlyCollection<FileIngestionResult>> RunWithBulkLogAsync(
        string endpointName,
        Func<Task<IReadOnlyCollection<FileIngestionResult>>> run,
        Dictionary<string, string> metadata,
        CancellationToken cancellationToken)
    {
        var startedAt = DateTime.Now;
        var actor = ResolveActor();
        var steps = new List<string>();
        var success = false;
        try
        {
            var results = await run();
            var completed = results.Count(x => string.Equals(x.Status, FileIngestionResultStatuses.Completed, StringComparison.OrdinalIgnoreCase));
            var failed = results.Count(x => string.Equals(x.Status, FileIngestionResultStatuses.Failed, StringComparison.OrdinalIgnoreCase));
            var skipped = results.Count(x => string.Equals(x.Status, FileIngestionResultStatuses.Skipped, StringComparison.OrdinalIgnoreCase));
            steps.Add($"ResultCount={results.Count},Completed={completed},Failed={failed},Skipped={skipped}");
            success = failed == 0;

            _logger.LogInformation("File ingestion endpoint completed. Endpoint={EndpointName}, ResultCount={ResultCount}, Completed={Completed}, Failed={Failed}, Skipped={Skipped}",
                endpointName, results.Count, completed, failed, skipped);
            return results;
        }
        catch (Exception ex)
        {
            steps.Add($"Exception={ex.Message}");
            _logger.LogError(ex, "File ingestion endpoint failed. Endpoint={EndpointName}", endpointName);
            throw;
        }
        finally
        {
            var endedAt = DateTime.Now;
            await _bulkLogPublisher.PublishAsync(new BulkServiceOperationLog
            {
                ServiceName = nameof(FileIngestionService),
                EndpointName = endpointName,
                Actor = actor,
                StartedAt = startedAt,
                EndedAt = endedAt,
                DurationMs = (long)(endedAt - startedAt).TotalMilliseconds,
                IsSuccess = success,
                Summary = success ? "File ingestion endpoint completed." : "File ingestion endpoint failed.",
                Metadata = metadata,
                Logs = steps
            }, cancellationToken);
        }
    }

    private async Task<IReadOnlyCollection<FileIngestionResult>> ImportFromLocalAsync(
        string directoryPath,
        bool resetExisting,
        FileIngestionResetScope resetScope,
        Func<string, bool> fileFilter,
        CancellationToken cancellationToken,
        bool enforceMaxFilesPerRun = true,
        int? maxFilesLimit = null)
    {
        if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
        {
            return [];
        }

        var files = Directory
            .GetFiles(directoryPath)
            .Where(IsSupportedInputFile)
            .Where(x => fileFilter(Path.GetFileName(x)))
            .OrderBy(x => x)
            .ToArray();

        if (enforceMaxFilesPerRun && _settings.MaxFilesPerRun > 0)
        {
            files = files.Take(_settings.MaxFilesPerRun).ToArray();
        }
        else if (maxFilesLimit.HasValue && maxFilesLimit.Value >= 0)
        {
            files = files.Take(maxFilesLimit.Value).ToArray();
        }

        var result = new List<FileIngestionResult>(files.Length);
        var encoding = ResolveEncoding();
        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);
            FileIngestionResult importResult;
            byte[] fileBytes = [];
            try
            {
                fileBytes = await File.ReadAllBytesAsync(file, cancellationToken);
                var content = encoding.GetString(fileBytes);
                importResult = await IngestFileAsync(
                    fileName,
                    content,
                    FileIngestionSourceTypes.Local,
                    file,
                    resetExisting,
                    resetScope,
                    ResolveActor(),
                    cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "File import failed before ingestion processing. FileName={FileName}, SourceType={SourceType}",
                    fileName, FileIngestionSourceTypes.Local);
                await RaiseFileImportFailedAlarmSafeAsync(
                    fileName,
                    DetectFileTypeFromName(fileName),
                    FileIngestionSourceTypes.Local,
                    ex.Message,
                    cancellationToken);

                importResult = new FileIngestionResult
                {
                    FileName = fileName,
                    FileType = DetectFileTypeFromName(fileName),
                    Status = FileIngestionResultStatuses.Failed,
                    Message = ex.Message
                };
            }

            result.Add(importResult);
            if (fileBytes.Length > 0)
            {
                try
                {
                    await ArchiveImportedFileAsync(
                        importResult,
                        Path.GetDirectoryName(file) ?? directoryPath,
                        isLocalSource: true,
                        fileName,
                        fileBytes,
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Archive copy failed after local import. FilePath={FilePath}, ImportStatus={ImportStatus}",
                        file, importResult.Status);
                }
            }
        }

        return result;
    }

    private async Task<IReadOnlyCollection<FileIngestionResult>> ImportFromFtpFileNamesAsync(
        IReadOnlyCollection<string> fileNames,
        string sourcePath,
        bool resetExisting,
        FileIngestionResetScope resetScope,
        CancellationToken cancellationToken,
        bool enforceMaxFilesPerRun = true)
    {
        var selected = fileNames.OrderBy(x => x).ToArray();
        if (enforceMaxFilesPerRun && _settings.MaxFilesPerRun > 0)
        {
            selected = selected.Take(_settings.MaxFilesPerRun).ToArray();
        }

        var result = new List<FileIngestionResult>(selected.Length);
        var encoding = ResolveEncoding();
        foreach (var fileName in selected)
        {
            FileIngestionResult importResult;
            byte[] fileBytes = [];
            try
            {
                var content = await _remoteFileFetcher.ReadFileTextAsync(sourcePath, fileName, encoding, cancellationToken);
                fileBytes = encoding.GetBytes(content);
                importResult = await IngestFileAsync(
                    fileName,
                    content,
                    FileIngestionSourceTypes.Ftp,
                    sourcePath,
                    resetExisting,
                    resetScope,
                    ResolveActor(),
                    cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "File import failed before ingestion processing. FileName={FileName}, SourceType={SourceType}",
                    fileName, FileIngestionSourceTypes.Ftp);
                await RaiseFileImportFailedAlarmSafeAsync(
                    fileName,
                    DetectFileTypeFromName(fileName),
                    FileIngestionSourceTypes.Ftp,
                    ex.Message,
                    cancellationToken);

                importResult = new FileIngestionResult
                {
                    FileName = fileName,
                    FileType = DetectFileTypeFromName(fileName),
                    Status = FileIngestionResultStatuses.Failed,
                    Message = ex.Message
                };
            }

            result.Add(importResult);
            if (fileBytes.Length > 0)
            {
                try
                {
                    await ArchiveImportedFileAsync(
                        importResult,
                        sourcePath,
                        isLocalSource: false,
                        fileName,
                        fileBytes,
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Archive copy failed after remote import. FileName={FileName}, SourcePath={SourcePath}, ImportStatus={ImportStatus}",
                        fileName, sourcePath, importResult.Status);
                }
            }
        }

        return result;
    }

    private async Task RaiseFileImportFailedAlarmSafeAsync(
        string fileName,
        string fileType,
        string sourceType,
        string error,
        CancellationToken cancellationToken)
    {
        await RaiseAlarmSafeAsync(
            FileIngestionAlarmCodes.FileImportFailed,
            "Card file import failed.",
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["fileName"] = fileName,
                ["fileType"] = fileType,
                ["sourceType"] = sourceType,
                ["error"] = error
            },
            cancellationToken);
    }

    private async Task RaiseAlarmSafeAsync(
        string alarmCode,
        string summary,
        IReadOnlyDictionary<string, string> metadata,
        CancellationToken cancellationToken)
    {
        try
        {
            await _alarmService.RaiseAsync(alarmCode, summary, metadata, cancellationToken);
        }
        catch (Exception alarmEx)
        {
            _logger.LogError(alarmEx, "Alarm raise failed. AlarmCode={AlarmCode}", alarmCode);
        }
    }

    private async Task ArchiveImportedFileAsync(
        FileIngestionResult importResult,
        string sourceDirectoryPath,
        bool isLocalSource,
        string fileName,
        byte[] content,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(fileName) || content.Length == 0)
        {
            return;
        }

        var statusExtension = string.Equals(importResult.Status, FileIngestionResultStatuses.Failed, StringComparison.OrdinalIgnoreCase)
            ? "failed"
            : "processed";
        var archivedFileName = BuildArchivedFileName(fileName, statusExtension);
        const string relativeDirectoryPath = "ARCHIVE";

        var localArchiveError = await WriteArchiveLocalAsync(sourceDirectoryPath, isLocalSource, relativeDirectoryPath, archivedFileName, content, cancellationToken);
        var remoteArchiveError = await WriteArchiveRemoteAsync(relativeDirectoryPath, archivedFileName, content, cancellationToken);

        if (!localArchiveError && !remoteArchiveError)
        {
            return;
        }

        importResult.HasErrors = true;
        var errorMessage = "Archive write failed.";
        importResult.Message = string.IsNullOrWhiteSpace(importResult.Message)
            ? errorMessage
            : $"{importResult.Message} {errorMessage}";
    }

    private async Task<bool> WriteArchiveLocalAsync(
        string sourceDirectoryPath,
        bool isLocalSource,
        string relativeDirectoryPath,
        string fileName,
        byte[] content,
        CancellationToken cancellationToken)
    {
        if (_settings.Local.ArchiveEnabled != true || !isLocalSource || string.IsNullOrWhiteSpace(sourceDirectoryPath))
        {
            return false;
        }

        try
        {
            var targetDirectory = Path.Combine(sourceDirectoryPath, relativeDirectoryPath.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(targetDirectory);
            var targetFilePath = Path.Combine(targetDirectory, fileName);
            await File.WriteAllBytesAsync(targetFilePath, content, cancellationToken);
            File.SetLastWriteTime(targetFilePath, DateTime.Now);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Local archive write failed. RelativeDirectoryPath={RelativeDirectoryPath}, FileName={FileName}",
                relativeDirectoryPath, fileName);
            return true;
        }
    }

    private async Task<bool> WriteArchiveRemoteAsync(
        string relativeDirectoryPath,
        string fileName,
        byte[] content,
        CancellationToken cancellationToken)
    {
        try
        {
            var enabled = await _archiveRemoteFileWriter.IsEnabledAsync(cancellationToken);
            if (!enabled)
            {
                return false;
            }

            var uploaded = await _archiveRemoteFileWriter.WriteFileAsync(relativeDirectoryPath, fileName, content, cancellationToken);
            return !uploaded;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Remote archive write failed. RelativeDirectoryPath={RelativeDirectoryPath}, FileName={FileName}",
                relativeDirectoryPath, fileName);
            return true;
        }
    }

    private string ResolveArchiveTimestampFormat()
    {
        var configured = (_settings.TimestampFormat ?? string.Empty).Trim();
        return string.IsNullOrWhiteSpace(configured) ? "yyyyMMdd_HHmmss" : configured;
    }

    private string BuildArchivedFileName(string originalFileName, string statusExtension)
    {
        var timestamp = DateTime.Now.ToString(ResolveArchiveTimestampFormat(), CultureInfo.InvariantCulture);
        var fileName = string.IsNullOrWhiteSpace(originalFileName)
            ? "unknown"
            : originalFileName;
        var stem = Path.GetFileNameWithoutExtension(fileName);
        if (string.IsNullOrWhiteSpace(stem))
        {
            stem = fileName;
        }

        return $"{stem}_{timestamp}.{statusExtension}";
    }

    private static string ResolveLocalIncomingPath(string providedPath, string incomingFallback, string defaultDriveCode)
    {
        if (!string.IsNullOrWhiteSpace(providedPath))
        {
            return NormalizeLocalPath(providedPath, defaultDriveCode);
        }

        if (string.IsNullOrWhiteSpace(incomingFallback))
        {
            return incomingFallback;
        }

        return NormalizeLocalPath(incomingFallback, defaultDriveCode);
    }

    private IReadOnlyCollection<string> ResolveLocalClearingIncomingPaths(string providedPath, LocalIngestionSettings sourceLocal)
    {
        if (!string.IsNullOrWhiteSpace(providedPath))
        {
            return [NormalizeLocalPath(providedPath, sourceLocal.DefaultDriveCode)];
        }

        var todayFolder = DateTime.Now.ToString("yyMMdd", CultureInfo.InvariantCulture);
        var candidates = new[]
        {
            ResolveConfiguredOrOsDefaultPath(sourceLocal.BkmClearingPath, defaults => defaults.BkmClearingPath, sourceLocal.Defaults),
            ResolveConfiguredOrOsDefaultPath(sourceLocal.MastercardClearingPath, defaults => defaults.MastercardClearingPath, sourceLocal.Defaults),
            ResolveConfiguredOrOsDefaultPath(sourceLocal.VisaClearingPath, defaults => defaults.VisaClearingPath, sourceLocal.Defaults)
        };

        return candidates
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(path => NormalizeLocalPath(path, sourceLocal.DefaultDriveCode))
            .Select(path => EndsWithDateFolder(path)
                ? path
                : Path.Combine(path, todayFolder))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string NormalizeLocalPath(string path, string defaultDriveCode)
    {
        var raw = (path ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(raw))
        {
            return raw;
        }

        if (OperatingSystem.IsWindows())
        {
            var normalizedWindows = raw.Replace('/', '\\');
            if (normalizedWindows.StartsWith("\\") && !normalizedWindows.StartsWith("\\\\"))
            {
                var drive = string.IsNullOrWhiteSpace(defaultDriveCode)
                    ? "C:"
                    : defaultDriveCode.TrimEnd(':') + ":";
                normalizedWindows = drive + normalizedWindows;
            }

            return Path.GetFullPath(normalizedWindows);
        }

        if (Regex.IsMatch(raw, "^[A-Za-z]:[\\\\/]", RegexOptions.CultureInvariant))
        {
            throw new InvalidOperationException(
                $"Local path format '{raw}' is Windows-specific but current OS is '{GetCurrentOsName()}'. Configure OS-appropriate local paths.");
        }

        var normalizedUnix = raw.Replace('\\', '/');
        return Path.GetFullPath(normalizedUnix);
    }

    private static string ResolveConfiguredOrOsDefaultPath(
        string configuredPath,
        Func<LocalOsPathSettings, string> selector,
        LocalPathDefaults defaults)
    {
        if (!string.IsNullOrWhiteSpace(configuredPath))
        {
            return configuredPath;
        }

        var osDefaults = ResolveCurrentOsDefaults(defaults);
        return selector(osDefaults);
    }

    private static LocalOsPathSettings ResolveCurrentOsDefaults(LocalPathDefaults defaults)
    {
        if (defaults is null)
        {
            return new LocalOsPathSettings();
        }

        if (OperatingSystem.IsWindows())
        {
            return defaults.Windows ?? new LocalOsPathSettings();
        }

        if (OperatingSystem.IsMacOS())
        {
            return defaults.MacOS ?? new LocalOsPathSettings();
        }

        return defaults.Linux ?? new LocalOsPathSettings();
    }

    private static string GetCurrentOsName()
    {
        if (OperatingSystem.IsWindows())
        {
            return "Windows";
        }

        if (OperatingSystem.IsMacOS())
        {
            return "MacOS";
        }

        if (OperatingSystem.IsLinux())
        {
            return "Linux";
        }

        return "Unknown";
    }

    private static bool EndsWithDateFolder(string path)
    {
        var directoryName = Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        return !string.IsNullOrWhiteSpace(directoryName)
               && Regex.IsMatch(directoryName, "^\\d{6}$", RegexOptions.CultureInvariant);
    }

    private bool IsSupportedInputFile(string fullPath)
    {
        var ext = Path.GetExtension(fullPath);
        return _settings.FileDetection.SupportedExtensions.Any(x => x.Equals(ext, StringComparison.OrdinalIgnoreCase));
    }

    private bool IsCardFileName(string fileName)
    {
        return MatchesAnyFileNamePattern(fileName, _settings.FileDetection.CardFileNamePatterns);
    }

    private bool IsClearingFileName(string fileName)
    {
        return MatchesAnyFileNamePattern(fileName, _settings.FileDetection.ClearingFileNamePatterns);
    }

    private string ResolveActor()
    {
        var userId = _contextProvider?.CurrentContext?.UserId;
        if (!string.IsNullOrWhiteSpace(userId))
        {
            return userId;
        }

        return AuditUsers.CardFileIngestion;
    }

    private Encoding ResolveEncoding()
    {
        try
        {
            return Encoding.GetEncoding(_settings.FileEncoding ?? FileIngestionValues.DefaultEncoding);
        }
        catch
        {
            return Encoding.UTF8;
        }
    }

    private static readonly ConcurrentDictionary<string, SemaphoreSlim> FileLocks = new();
    private const int BulkInsertBatchSize = 1000;

    private async Task<FileIngestionResult> IngestFileAsync(
        string fileName,
        string content,
        string sourceType,
        string sourcePath,
        bool resetExisting,
        FileIngestionResetScope resetScope,
        string actor,
        CancellationToken cancellationToken)
    {
        var detectedFileType = DetectFileTypeFromName(fileName);
        var lockKey = $"{sourceType}:{fileName}";
        var fileLock = FileLocks.GetOrAdd(lockKey, _ => new SemaphoreSlim(1, 1));
        await fileLock.WaitAsync(cancellationToken);

        try
        {
            var normalizedContent = NormalizeFileContent(content);
            var hash = ComputeContentHash(normalizedContent);
            var parser = _parsers.FirstOrDefault(x => x.CanParse(fileName));
            if (parser is null)
            {
                return new FileIngestionResult
                {
                    FileName = fileName,
                    FileType = detectedFileType,
                    HasErrors = true,
                    Status = FileIngestionResultStatuses.Failed,
                    Message = "No parser matched this file"
                };
            }

            var lines = SplitLines(normalizedContent);
            IReadOnlyCollection<ParsedFileRecord> parsedRecords;
            FileIngestionResult result = null;
            var autoOperationCountToTrigger = 0;
            var ingestionCommitted = false;
            Guid? importedFileId = null;

            try
            {
                parsedRecords = parser.Parse(fileName, lines);

                if (resetExisting)
                {
                    if (resetScope == FileIngestionResetScope.FileName)
                    {
                        await ResetByFileNameAsync(fileName, cancellationToken);
                    }
                    else
                    {
                        await ResetByHashAsync(hash, cancellationToken);
                    }
                }

                var existing = await _dbContext.ImportedFiles
                    .AsNoTracking()
                    .Where(x => x.FileHash == hash)
                    .Select(x => x.FileType)
                    .FirstOrDefaultAsync(cancellationToken);

                if (!string.IsNullOrWhiteSpace(existing))
                {
                    result = new FileIngestionResult
                    {
                        FileName = fileName,
                        FileType = existing,
                        Status = FileIngestionResultStatuses.Skipped,
                        Message = "File hash already processed"
                    };
                }
                else
                {
                    var (declaredFileDate, declaredFileNo, declaredFileVersionNumber) = ExtractDeclaredIdentity(parsedRecords);
                    if (!string.IsNullOrWhiteSpace(declaredFileDate)
                        && !string.IsNullOrWhiteSpace(declaredFileNo)
                        && !string.IsNullOrWhiteSpace(declaredFileVersionNumber))
                    {
                        var semanticDuplicate = await _dbContext.ImportedFiles
                            .AsNoTracking()
                            .Where(x => x.FileFamily == (parser.FileType.Equals(FileIngestionValues.CardFileType, StringComparison.OrdinalIgnoreCase)
                                ? FileFamily.CardTransaction
                                : FileFamily.Clearing))
                            .Where(x => x.SourceType == sourceType)
                            .Where(x => x.DeclaredFileDate == declaredFileDate)
                            .Where(x => x.DeclaredFileNo == declaredFileNo)
                            .Where(x => x.DeclaredFileVersionNumber == declaredFileVersionNumber)
                            .Select(x => x.Id)
                            .FirstOrDefaultAsync(cancellationToken);

                        if (semanticDuplicate != Guid.Empty)
                        {
                            result = new FileIngestionResult
                            {
                                FileName = fileName,
                                FileType = parser.FileType,
                                Status = FileIngestionResultStatuses.Skipped,
                                Message = $"Semantic duplicate detected. Identity={declaredFileDate}/{declaredFileNo}/{declaredFileVersionNumber}"
                            };
                        }
                    }

                    if (result is null)
                    {
                        var importedFile = new ImportedFile
                        {
                            Id = Guid.NewGuid(),
                            FileName = fileName,
                            FileFamily = parser.FileType.Equals(FileIngestionValues.CardFileType, StringComparison.OrdinalIgnoreCase)
                                ? FileFamily.CardTransaction
                                : FileFamily.Clearing,
                            FileType = parser.FileType,
                            SourceType = sourceType,
                            SourcePath = sourcePath,
                            FileHash = hash,
                            Status = ImportedFileStatus.Parsed,
                            TotalLineCount = lines.Length,
                            CreateDate = DateTime.Now,
                            CreatedBy = actor,
                            RecordStatus = RecordStatus.Active
                        };
                        importedFileId = importedFile.Id;

                        PopulateIncomingFileMetadata(importedFile, parsedRecords);

                        await ExecuteInDbTransactionAsync(async ct =>
                        {
                            await _dbContext.ImportedFiles.AddAsync(importedFile, ct);
                            await _dbContext.SaveChangesAsync(ct);

                            var rowMap = await WriteRowsAsync(importedFile.Id, parsedRecords, actor, ct);
                            await WriteBusinessRecordsAsync(importedFile, rowMap, parsedRecords, fileName, actor, ct);

                            await _dbContext.ImportedFiles
                                .Where(x => x.Id == importedFile.Id)
                                .ExecuteUpdateAsync(setters => setters
                                    .SetProperty(x => x.Status, ImportedFileStatus.Completed)
                                    .SetProperty(x => x.ProcessedAt, DateTime.Now), ct);
                        }, cancellationToken);
                        ingestionCommitted = true;

                        try
                        {
                            var reconciliation = await _reconciliationService.ProcessImportedFileAsync(
                                importedFile.Id,
                                actor,
                                cancellationToken: cancellationToken);
                            if (importedFile.FileFamily == FileFamily.Clearing)
                            {
                                var reprocessed = await _reconciliationService.RegenerateOperationsAsync(
                                    actor,
                                    importedFileId: importedFile.Id,
                                    cancellationToken: cancellationToken);
                                reconciliation.ReconciledCards += reprocessed.ReconciledCards;
                                reconciliation.ReconciliationOperations += reprocessed.ReconciliationOperations;
                                reconciliation.ReconciliationManualReviewItems += reprocessed.ReconciliationManualReviewItems;
                            }

                            var plannedAutoOperationCount = Math.Max(0,
                                reconciliation.ReconciliationOperations - reconciliation.ReconciliationManualReviewItems);
                            autoOperationCountToTrigger = plannedAutoOperationCount;

                            result = new FileIngestionResult
                            {
                                FileName = fileName,
                                FileType = parser.FileType,
                                TotalLines = lines.Length,
                                ParsedRecords = parsedRecords.Count,
                                ReconciledCards = reconciliation.ReconciledCards,
                                ReconciliationOperations = reconciliation.ReconciliationOperations,
                                ReconciliationManualReviewItems = reconciliation.ReconciliationManualReviewItems,
                                PlannedAutoOperations = plannedAutoOperationCount,
                                Status = FileIngestionResultStatuses.Completed,
                                Message = $"File imported successfully. reconciled_cards={reconciliation.ReconciledCards}, reconciliation_operations={reconciliation.ReconciliationOperations}, reconciliation_manual_review_items={reconciliation.ReconciliationManualReviewItems}"
                            };
                        }
                        catch (Exception reconciliationEx)
                        {
                            _logger.LogError(
                                reconciliationEx,
                                "Reconciliation failed after ingestion commit. FileName={FileName}, ImportedFileId={ImportedFileId}",
                                fileName,
                                importedFile.Id);

                            await RaiseAlarmSafeAsync(
                                FileIngestionAlarmCodes.ReconciliationFailedAfterImport,
                                "Reconciliation failed after file ingestion commit.",
                                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                                {
                                    ["fileName"] = fileName,
                                    ["fileType"] = parser.FileType,
                                    ["sourceType"] = sourceType,
                                    ["importedFileId"] = importedFile.Id.ToString(),
                                    ["error"] = reconciliationEx.Message
                                },
                                cancellationToken);

                            // Ingestion committed successfully. Keep file as completed/processed,
                            // but expose reconciliation failure in operation message.
                            result = new FileIngestionResult
                            {
                                FileName = fileName,
                                FileType = parser.FileType,
                                TotalLines = lines.Length,
                                ParsedRecords = parsedRecords.Count,
                                ReconciledCards = 0,
                                ReconciliationOperations = 0,
                                ReconciliationManualReviewItems = 0,
                                HasErrors = true,
                                Status = FileIngestionResultStatuses.Completed,
                                Message = $"File imported successfully, but reconciliation failed: {reconciliationEx.Message}"
                            };
                        }
                    }
                }
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogWarning(dbEx, "Concurrent file ingestion detected. FileName={FileName}", fileName);

                var existingType = await _dbContext.ImportedFiles
                    .AsNoTracking()
                    .Where(x => x.FileHash == hash)
                    .Select(x => x.FileType)
                    .FirstOrDefaultAsync(cancellationToken);

                result = new FileIngestionResult
                {
                    FileName = fileName,
                    FileType = string.IsNullOrWhiteSpace(existingType) ? detectedFileType : existingType,
                    Status = FileIngestionResultStatuses.Skipped,
                    Message = "File already processed (concurrent run)"
                };
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                if (ingestionCommitted)
                {
                    try
                    {
                        await ResetByHashAsync(hash, cancellationToken);
                        _logger.LogWarning(
                            "Ingestion rollback executed after failure. FileName={FileName}, ImportedFileId={ImportedFileId}, FileHash={FileHash}",
                            fileName,
                            importedFileId,
                            hash);
                    }
                    catch (Exception rollbackEx)
                    {
                        _logger.LogError(
                            rollbackEx,
                            "Ingestion rollback failed after file import failure. FileName={FileName}, ImportedFileId={ImportedFileId}, FileHash={FileHash}",
                            fileName,
                            importedFileId,
                            hash);
                    }
                }

                _logger.LogError(ex, "File import failed. FileName={FileName}", fileName);
                await RaiseAlarmSafeAsync(
                    FileIngestionAlarmCodes.FileImportFailed,
                    "Card file import failed.",
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["fileName"] = fileName,
                        ["fileType"] = parser.FileType,
                        ["sourceType"] = sourceType,
                        ["error"] = ex.Message
                    },
                    cancellationToken);

                result = new FileIngestionResult
                {
                    FileName = fileName,
                    FileType = parser.FileType,
                    HasErrors = true,
                    Status = FileIngestionResultStatuses.Failed,
                    Message = ex.Message
                };
            }

            var autoOperationTriggerEnabled = _settings.ReconciliationProcessing?.EnableAutoOperationTrigger != false;
            var isRemoteImport = string.Equals(sourceType, FileIngestionSourceTypes.Ftp, StringComparison.OrdinalIgnoreCase);
            var autoExecutionTriggered = false;
            if (autoOperationTriggerEnabled && isRemoteImport && autoOperationCountToTrigger > 0)
            {
                try
                {
                    var autoOperationActor = _contextProvider?.CurrentContext?.UserId ?? AuditUsers.CardFileIngestion;
                    await _reconciliationAutoOperationService.ExecutePendingOperationsAsync(
                        autoOperationCountToTrigger,
                        autoOperationActor,
                        cancellationToken);
                    autoExecutionTriggered = true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Auto operation trigger failed after ingestion commit. FileName={FileName}, AutoOperationCount={AutoOperationCount}",
                        fileName,
                        autoOperationCountToTrigger);

                    await RaiseAlarmSafeAsync(
                        FileIngestionAlarmCodes.ReconciliationAutoActionTriggerFailed,
                        "Reconciliation auto operation trigger failed after file ingestion commit.",
                        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                        {
                            ["fileName"] = fileName,
                            ["fileType"] = result?.FileType ?? detectedFileType,
                            ["sourceType"] = sourceType,
                            ["autoOperationCount"] = autoOperationCountToTrigger.ToString(CultureInfo.InvariantCulture),
                            ["error"] = ex.Message
                        },
                        cancellationToken);

                    if (result is not null && string.Equals(result.Status, FileIngestionResultStatuses.Completed, StringComparison.OrdinalIgnoreCase))
                    {
                        result.HasErrors = true;
                        result.Message = string.IsNullOrWhiteSpace(result.Message)
                            ? "File imported successfully, but auto operation trigger failed."
                            : $"{result.Message} Auto operation trigger failed: {ex.Message}";
                    }
                }
            }
            else if (autoOperationCountToTrigger > 0
                     && !isRemoteImport
                     && result is not null
                     && string.Equals(result.Status, FileIngestionResultStatuses.Completed, StringComparison.OrdinalIgnoreCase))
            {
                result.Message = string.IsNullOrWhiteSpace(result.Message)
                    ? "File imported successfully. Auto operations are waiting for manual API execution."
                    : $"{result.Message} Auto operations are waiting for manual API execution.";
            }

            if (result is not null)
            {
                result.AutoExecutionTriggered = autoExecutionTriggered;
                result.PendingAutoOperations = autoOperationCountToTrigger > 0 && !autoExecutionTriggered
                    ? autoOperationCountToTrigger
                    : 0;
            }

            return result ?? new FileIngestionResult
            {
                FileName = fileName,
                FileType = parser.FileType,
                AutoExecutionTriggered = false,
                PendingAutoOperations = 0,
                PlannedAutoOperations = 0,
                HasErrors = true,
                Status = FileIngestionResultStatuses.Failed,
                Message = "File import result could not be resolved"
            };
        }
        finally
        {
            fileLock.Release();
            if (fileLock.CurrentCount == 1)
            {
                FileLocks.TryRemove(lockKey, out _);
            }
        }
    }

    private async Task<Dictionary<int, Guid>> WriteRowsAsync(
        Guid importedFileId,
        IReadOnlyCollection<ParsedFileRecord> parsedRecords,
        string actor,
        CancellationToken cancellationToken)
    {
        var records = parsedRecords as ParsedFileRecord[] ?? parsedRecords.ToArray();
        var rowMap = new Dictionary<int, Guid>(records.Length);

        for (var index = 0; index < records.Length; index += BulkInsertBatchSize)
        {
            var batch = records.Skip(index).Take(BulkInsertBatchSize).ToArray();
            var rows = batch.Select(record =>
            {
                var rowId = Guid.NewGuid();
                rowMap[record.LineNo] = rowId;
                return new ImportedFileRow
                {
                    Id = rowId,
                    ImportedFileId = importedFileId,
                    LineNo = record.LineNo,
                    RowType = record.RecordType,
                    RawLine = record.RawLine,
                    ParsedJson = JsonSerializer.Serialize(record.Fields),
                    CreateDate = DateTime.Now,
                    CreatedBy = actor,
                    RecordStatus = RecordStatus.Active
                };
            }).ToArray();

            await _dbContext.ImportedFileRows.AddRangeAsync(rows, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            _dbContext.ChangeTracker.Clear();
        }

        return rowMap;
    }

    private async Task WriteBusinessRecordsAsync(
        ImportedFile importedFile,
        Dictionary<int, Guid> rowMap,
        IReadOnlyCollection<ParsedFileRecord> parsedRecords,
        string fileName,
        string actor,
        CancellationToken cancellationToken)
    {
        var detailRecords = parsedRecords.Where(x => x.RecordType == FixedWidthRecordTypes.Detail).ToArray();

        if (importedFile.FileFamily == FileFamily.CardTransaction)
        {
            ValidateCardCodeDictionary(detailRecords, fileName);

            for (var index = 0; index < detailRecords.Length; index += BulkInsertBatchSize)
            {
                var batch = detailRecords.Skip(index).Take(BulkInsertBatchSize).ToArray();
                var cardRecords = batch.Select(d => new CardTransactionRecord
                {
                    Id = Guid.NewGuid(),
                    ImportedFileRowId = rowMap[d.LineNo],
                    TransactionDate = ParseDateYYYYMMDD(Get(d, "TransactionDate")),
                    TransactionTime = ParseTime(Get(d, "TransactionTime")),
                    ValueDate = ParseDateYYYYMMDD(Get(d, "ValueDate")),
                    EndOfDayDate = ParseDateYYYYMMDD(Get(d, "EndOfDayDate")),
                    CardNo = NormalizeKeyField(Get(d, "CardNo")),
                    OceanTxnGuid = NormalizeKeyField(Get(d, "OceanTxnGuid")),
                    OceanMainTxnGuid = NormalizeKeyField(Get(d, "OceanMainTxnGuid")),
                    BranchId = Get(d, "BranchID"),
                    Rrn = NormalizeKeyField(Get(d, "RRN")),
                    Arn = NormalizeKeyField(Get(d, "ARN")),
                    ProvisionCode = NormalizeKeyField(Get(d, "ProvisionCode")),
                    Stan = Get(d, "Stan"),
                    MemberRefNo = Get(d, "MemberRefNo"),
                    TraceId = Get(d, "TraceID"),
                    Otc = Get(d, "Otc"),
                    Ots = Get(d, "Ots"),
                    TxnInstallType = Get(d, "TxnInstallType"),
                    BankingTxnCode = Get(d, "BankingTxnCode"),
                    TxnDescription = Get(d, "TxnDescription"),
                    MerchantName = Get(d, "MerchantName"),
                    MerchantCity = Get(d, "MerchantCity"),
                    MerchantState = Get(d, "MerchantState"),
                    MerchantCountry = Get(d, "MerchantCountry"),
                    FinancialType = Get(d, "FinancialType"),
                    TxnEffect = Get(d, "TxnEffect"),
                    TxnSource = Get(d, "TxnSource"),
                    TxnRegion = Get(d, "TxnRegion"),
                    TerminalType = Get(d, "TerminalType"),
                    ChannelCode = Get(d, "ChannelCode"),
                    TerminalId = Get(d, "TerminalId"),
                    MerchantId = Get(d, "MerchantId"),
                    Mcc = NormalizeKeyField(Get(d, "Mcc")),
                    AcquirerId = Get(d, "AcquirerId"),
                    SecurityLevelIndicator = Get(d, "SecurityLevelIndicator"),
                    IsTxnSettle = NormalizeKeyField(Get(d, "IsTxnSettle")),
                    TxnStat = NormalizeKeyField(Get(d, "TxnStat")),
                    ResponseCode = NormalizeKeyField(Get(d, "ResponseCode")),
                    IsSuccessfulTxn = NormalizeKeyField(Get(d, "IsSuccessfulTxn")),
                    TxnOrigin = ParseTxnOrigin(Get(d, "TxnOrigin")),
                    InstallCount = ParseInt(Get(d, "InstallCount")),
                    InstallOrder = ParseInt(Get(d, "InstallOrder")),
                    OperatorCode = Get(d, "OperatorCode"),
                    OriginalAmount = ParseImplied2Decimal(Get(d, "OriginalAmount")),
                    OriginalCurrency = Get(d, "OriginalCurrency"),
                    SettlementAmount = ParseImplied2Decimal(Get(d, "SettlementAmount")),
                    SettlementCurrency = Get(d, "SettlementCurrency"),
                    CardHolderBillingAmount = ParseImplied2Decimal(Get(d, "CardHolderBillingAmount")),
                    CardHolderBillingCurrency = Get(d, "CardHolderBillingCurrency"),
                    BillingAmount = ParseImplied2Decimal(Get(d, "BillingAmount")),
                    BillingCurrency = Get(d, "BillingCurrency"),
                    Tax1 = ParseImplied2Decimal(Get(d, "Tax1")),
                    Tax2 = ParseImplied2Decimal(Get(d, "Tax2")),
                    CashbackAmount = ParseImplied2Decimal(Get(d, "CashbackAmount")),
                    SurchargeAmount = ParseImplied2Decimal(Get(d, "SurchargeAmount")),
                    PointType = Get(d, "PointType"),
                    BcPoint = ParseImplied2Decimal(Get(d, "BcPoint")),
                    McPoint = ParseImplied2Decimal(Get(d, "McPoint")),
                    CcPoint = ParseImplied2Decimal(Get(d, "CcPoint")),
                    BcPointAmount = ParseImplied2Decimal(Get(d, "BcPointAmount")),
                    McPointAmount = ParseImplied2Decimal(Get(d, "McPointAmount")),
                    CcPointAmount = ParseImplied2Decimal(Get(d, "CcPointAmount")),
                    CorrelationKey = BuildCorrelationKey(
                        oceanTxnGuid: Get(d, "OceanTxnGuid"),
                        rrn: Get(d, "RRN"),
                        cardNo: Get(d, "CardNo"),
                        provisionCode: Get(d, "ProvisionCode"),
                        arn: Get(d, "ARN"),
                        mcc: Get(d, "Mcc"),
                        amount: FirstNonEmpty(Get(d, "OriginalAmount"), Get(d, "SettlementAmount"), Get(d, "CardHolderBillingAmount")),
                        currency: FirstNonEmpty(Get(d, "OriginalCurrency"), Get(d, "SettlementCurrency"), Get(d, "CardHolderBillingCurrency"))),
                    ReconciliationState = CardReconciliationState.AwaitingReevaluation,
                    ReconciliationStateUpdatedAt = DateTime.Now,
                    LastReconciliationRunId = null,
                    LastReconciliationExecutionGroupId = null,
                    ReconciliationStateReason = ReconciliationStateReasons.NewCardIngested,
                    CreateDate = DateTime.Now,
                    CreatedBy = actor,
                    RecordStatus = RecordStatus.Active
                }).ToArray();

                await _dbContext.CardTransactionRecords.AddRangeAsync(cardRecords, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
                _dbContext.ChangeTracker.Clear();
            }

            return;
        }

        var provider = ResolveProvider(fileName);
        await ValidateBkmClrNoUniquenessAsync(detailRecords, fileName, provider, cancellationToken);
        ValidateClearingCodeDictionary(detailRecords, fileName, provider);
        for (var index = 0; index < detailRecords.Length; index += BulkInsertBatchSize)
        {
            var batch = detailRecords.Skip(index).Take(BulkInsertBatchSize).ToArray();
            var clearingRecords = batch.Select(d => new ClearingRecord
            {
                Id = Guid.NewGuid(),
                ImportedFileRowId = rowMap[d.LineNo],
                Provider = provider,
                TxnType = Get(d, "TxnType"),
                IoDate = ParseDateYYYYMMDD(Get(d, "IoDate")),
                IoFlag = Get(d, "IoFlag"),
                OceanTxnGuid = NormalizeKeyField(Get(d, "OceanTxnGuid")),
                ClrNo = Get(d, "ClrNo"),
                Rrn = NormalizeKeyField(Get(d, "Rrn")),
                Arn = NormalizeKeyField(Get(d, "Arn")),
                ReasonCode = Get(d, "ReasonCode"),
                Reserved = Get(d, "Reserved"),
                ProvisionCode = NormalizeKeyField(Get(d, "ProvisionCode")),
                CardNo = NormalizeKeyField(Get(d, "CardNo")),
                CardDci = Get(d, "CardDCI"),
                MccCode = NormalizeKeyField(Get(d, "MCCCode")),
                Mtid = Get(d, "Mtid"),
                FunctionCode = Get(d, "FunctionCode"),
                ProcessCode = Get(d, "ProcessCode"),
                ReversalIndicator = Get(d, "ReversalIndicator"),
                Tc = Get(d, "TC"),
                UsageCode = Get(d, "UsageCode"),
                DisputeCode = Get(d, "DisputeCode"),
                ControlStat = NormalizeKeyField(Get(d, "ControlStat")),
                SourceAmount = ParseImplied2Decimal(Get(d, "SourceAmount")),
                SourceCurrency = NormalizeKeyField(Get(d, "SourceCurrency")),
                DestinationAmount = ParseImplied2Decimal(Get(d, "DestinationAmount")),
                DestinationCurrency = Get(d, "DestinationCurrency"),
                CashbackAmount = ParseImplied2Decimal(Get(d, "CashbackAmount")),
                ReimbursementAmount = ParseImplied2Decimal(Get(d, "ReimbursementAmount")),
                ReimbursementAttribute = Get(d, "ReimbursementAttribute"),
                AncillaryTransactionCode = Get(d, "AncillaryTransactionCode"),
                AncillaryTransactionAmount = Get(d, "AncillaryTransactionAmount"),
                MicrofilmNumber = Get(d, "MicrofilmNumber"),
                MerchantCity = Get(d, "MerchantCity"),
                MerchantName = Get(d, "MerchantName"),
                CardAcceptorId = Get(d, "CardAcceptorId"),
                TxnDate = ParseDateYYYYMMDD(Get(d, "TxnDate")),
                TxnTime = ParseTime(Get(d, "TxnTime")),
                FileId = Get(d, "FileId"),
                CorrelationKey = BuildCorrelationKey(
                    oceanTxnGuid: Get(d, "OceanTxnGuid"),
                    rrn: Get(d, "Rrn"),
                    cardNo: Get(d, "CardNo"),
                    provisionCode: Get(d, "ProvisionCode"),
                    arn: Get(d, "Arn"),
                    mcc: Get(d, "MCCCode"),
                    amount: FirstNonEmpty(Get(d, "SourceAmount"), Get(d, "DestinationAmount")),
                    currency: FirstNonEmpty(Get(d, "SourceCurrency"), Get(d, "DestinationCurrency"))),
                CreateDate = DateTime.Now,
                CreatedBy = actor,
                RecordStatus = RecordStatus.Active
            }).ToArray();

            await _dbContext.ClearingRecords.AddRangeAsync(clearingRecords, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            _dbContext.ChangeTracker.Clear();

            var correlationKeys = clearingRecords
                .Select(x => x.CorrelationKey)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            if (correlationKeys.Length > 0)
            {
                var now = DateTime.Now;
                await _dbContext.CardTransactionRecords
                    .Where(x => x.CorrelationKey != null && correlationKeys.Contains(x.CorrelationKey))
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(x => x.ReconciliationState, CardReconciliationState.ReadyForReconcile)
                        .SetProperty(x => x.ReconciliationStateUpdatedAt, now)
                        .SetProperty(x => x.LastReconciliationRunId, (Guid?)null)
                        .SetProperty(x => x.LastReconciliationExecutionGroupId, (Guid?)null)
                        .SetProperty(x => x.ReconciliationStateReason, ReconciliationStateReasons.ClearingArrived)
                        .SetProperty(x => x.UpdateDate, now)
                        .SetProperty(x => x.LastModifiedBy, actor), cancellationToken);
            }
        }
    }

    private async Task ValidateBkmClrNoUniquenessAsync(
        IReadOnlyCollection<ParsedFileRecord> detailRecords,
        string fileName,
        ClearingProvider provider,
        CancellationToken cancellationToken)
    {
        if (provider != ClearingProvider.Bkm || detailRecords.Count == 0)
        {
            return;
        }

        var entries = detailRecords
            .Select(x => new
            {
                IoDate = ParseDateYYYYMMDD(Get(x, "IoDate")),
                ClrNo = NormalizeKeyField(Get(x, "ClrNo")),
                x.LineNo
            })
            .Where(x => x.IoDate.HasValue && !string.IsNullOrWhiteSpace(x.ClrNo))
            .ToList();

        if (entries.Count == 0)
        {
            return;
        }

        var duplicateInFile = entries
            .GroupBy(x => $"{x.IoDate:yyyyMMdd}|{x.ClrNo}", StringComparer.Ordinal)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToArray();

        if (duplicateInFile.Length > 0)
        {
            await RaiseAlarmSafeAsync(
                FileIngestionAlarmCodes.BkmClrNoDuplicateInFile,
                "Duplicate CLRNO detected in BKM clearing file for the same IoDate.",
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["fileName"] = fileName,
                    ["duplicateCount"] = duplicateInFile.Length.ToString(CultureInfo.InvariantCulture),
                    ["sampleKeys"] = string.Join(",", duplicateInFile.Take(20))
                },
                cancellationToken);

            throw new InvalidOperationException("BKM CLRNO uniqueness check failed: duplicate CLRNO exists in file for same IoDate.");
        }

        var ioDates = entries
            .Select(x => x.IoDate!.Value)
            .Distinct()
            .ToArray();
        var existing = await _dbContext.ClearingRecords
            .AsNoTracking()
            .Where(x => x.Provider == ClearingProvider.Bkm)
            .Where(x => x.IoDate.HasValue && ioDates.Contains(x.IoDate.Value))
            .Where(x => x.ClrNo != null)
            .Select(x => new
            {
                x.IoDate,
                x.ClrNo
            })
            .ToListAsync(cancellationToken);

        var currentKeys = entries
            .Select(x => $"{x.IoDate:yyyyMMdd}|{x.ClrNo}")
            .ToHashSet(StringComparer.Ordinal);

        var duplicateInDb = existing
            .Select(x => $"{(x.IoDate.HasValue ? x.IoDate.Value.ToString("yyyyMMdd", CultureInfo.InvariantCulture) : string.Empty)}|{NormalizeKeyField(x.ClrNo)}")
            .Where(currentKeys.Contains)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToArray();

        if (duplicateInDb.Length == 0)
        {
            return;
        }

        await RaiseAlarmSafeAsync(
            FileIngestionAlarmCodes.BkmClrNoDuplicateDb,
            "Duplicate CLRNO detected against existing BKM clearing records for the same IoDate.",
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["fileName"] = fileName,
                ["duplicateCount"] = duplicateInDb.Length.ToString(CultureInfo.InvariantCulture),
                ["sampleKeys"] = string.Join(",", duplicateInDb.Take(20))
            },
            cancellationToken);

        throw new InvalidOperationException("BKM CLRNO uniqueness check failed: duplicate CLRNO exists in database for same IoDate.");
    }

    private async Task ResetByHashAsync(string hash, CancellationToken cancellationToken)
    {
        await ExecuteInDbTransactionAsync(async ct =>
        {
            var fileIds = await _dbContext.ImportedFiles
                .Where(x => x.FileHash == hash)
                .Select(x => x.Id)
                .ToListAsync(ct);

            if (fileIds.Count == 0)
            {
                return;
            }

            var rowIds = await _dbContext.ImportedFileRows
                .Where(x => fileIds.Contains(x.ImportedFileId))
                .Select(x => x.Id)
                .ToListAsync(ct);

            await DeleteRelatedReconciliationArtifactsAsync(rowIds, ct);
            await _dbContext.CardTransactionRecords.Where(x => rowIds.Contains(x.ImportedFileRowId)).ExecuteDeleteAsync(ct);
            await _dbContext.ClearingRecords.Where(x => rowIds.Contains(x.ImportedFileRowId)).ExecuteDeleteAsync(ct);
            await _dbContext.ImportedFileRows.Where(x => fileIds.Contains(x.ImportedFileId)).ExecuteDeleteAsync(ct);
            await _dbContext.ImportedFiles.Where(x => fileIds.Contains(x.Id)).ExecuteDeleteAsync(ct);
        }, cancellationToken);
    }

    private async Task ResetByFileNameAsync(string fileName, CancellationToken cancellationToken)
    {
        await ExecuteInDbTransactionAsync(async ct =>
        {
            var fileIds = await _dbContext.ImportedFiles
                .Where(x => x.FileName == fileName)
                .Select(x => x.Id)
                .ToListAsync(ct);

            if (fileIds.Count == 0)
            {
                return;
            }

            var rowIds = await _dbContext.ImportedFileRows
                .Where(x => fileIds.Contains(x.ImportedFileId))
                .Select(x => x.Id)
                .ToListAsync(ct);

            await DeleteRelatedReconciliationArtifactsAsync(rowIds, ct);
            await _dbContext.CardTransactionRecords.Where(x => rowIds.Contains(x.ImportedFileRowId)).ExecuteDeleteAsync(ct);
            await _dbContext.ClearingRecords.Where(x => rowIds.Contains(x.ImportedFileRowId)).ExecuteDeleteAsync(ct);
            await _dbContext.ImportedFileRows.Where(x => fileIds.Contains(x.ImportedFileId)).ExecuteDeleteAsync(ct);
            await _dbContext.ImportedFiles.Where(x => fileIds.Contains(x.Id)).ExecuteDeleteAsync(ct);
        }, cancellationToken);
    }

    private async Task DeleteRelatedReconciliationArtifactsAsync(IReadOnlyCollection<Guid> rowIds, CancellationToken cancellationToken)
    {
        if (rowIds.Count == 0)
        {
            return;
        }

        var cardTransactionIds = await _dbContext.CardTransactionRecords
            .Where(x => rowIds.Contains(x.ImportedFileRowId))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var clearingIds = await _dbContext.ClearingRecords
            .Where(x => rowIds.Contains(x.ImportedFileRowId))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        if (cardTransactionIds.Count == 0 && clearingIds.Count == 0)
        {
            return;
        }

        await _dbContext.ReconciliationEvaluations
            .Where(x =>
                cardTransactionIds.Contains(x.CardTransactionRecordId) ||
                (x.ClearingRecordId.HasValue && clearingIds.Contains(x.ClearingRecordId.Value)))
            .ExecuteDeleteAsync(cancellationToken);

        var operationIds = await _dbContext.ReconciliationOperations
            .Where(x =>
                cardTransactionIds.Contains(x.CardTransactionRecordId) ||
                (x.ClearingRecordId.HasValue && clearingIds.Contains(x.ClearingRecordId.Value)))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        if (operationIds.Count > 0)
        {
            await _dbContext.ReconciliationManualReviewItems
                .Where(x => operationIds.Contains(x.ReconciliationOperationId))
                .ExecuteDeleteAsync(cancellationToken);

            await _dbContext.ReconciliationOperationExecutions
                .Where(x => operationIds.Contains(x.ReconciliationOperationId))
                .ExecuteDeleteAsync(cancellationToken);

            await _dbContext.ReconciliationOperations
                .Where(x => operationIds.Contains(x.Id))
                .ExecuteDeleteAsync(cancellationToken);
        }
    }

    private async Task ExecuteInDbTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken)
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                await operation(cancellationToken);
                await tx.CommitAsync(cancellationToken);
            }
            catch
            {
                await tx.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    private string DetectFileTypeFromName(string fileName)
    {
        if (IsCardFileName(fileName))
        {
            return FileIngestionValues.CardFileType;
        }

        if (IsClearingFileName(fileName))
        {
            return FileIngestionValues.ClearingFileType;
        }

        return FileIngestionValues.UnknownFileType;
    }

    private static string BuildCorrelationKey(
        string oceanTxnGuid,
        string rrn,
        string cardNo,
        string provisionCode,
        string arn,
        string mcc,
        string amount,
        string currency)
    {
        var normalizedOceanTxnGuid = NormalizeKeyField(oceanTxnGuid);
        if (IsUsableOceanTxnGuid(normalizedOceanTxnGuid))
        {
            return $"{CorrelationKeyValues.OceanTxnGuidPrefix}{CorrelationKeyValues.SegmentSeparator}{normalizedOceanTxnGuid}";
        }

        return string.Join(
            CorrelationKeyValues.SegmentSeparator,
            "SIG",
            NormalizeKeyField(rrn),
            NormalizeKeyField(cardNo),
            NormalizeKeyField(provisionCode),
            NormalizeKeyField(arn),
            NormalizeKeyField(mcc),
            NormalizeCorrelationAmount(amount),
            NormalizeKeyField(currency));
    }

    private static string FirstNonEmpty(params string[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return string.Empty;
    }

    private static bool IsUsableOceanTxnGuid(string normalizedOceanTxnGuid)
    {
        if (string.IsNullOrWhiteSpace(normalizedOceanTxnGuid))
        {
            return false;
        }

        return normalizedOceanTxnGuid.Any(ch => ch != '0');
    }

    private static string NormalizeCorrelationAmount(string amount)
    {
        var normalizedAmount = NormalizeKeyField(amount);
        if (string.IsNullOrWhiteSpace(normalizedAmount))
        {
            return string.Empty;
        }

        var implied = ParseImplied2Decimal(normalizedAmount);
        if (implied.HasValue)
        {
            return implied.Value.ToString("0.00", CultureInfo.InvariantCulture);
        }

        return decimal.TryParse(normalizedAmount, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed)
            ? parsed.ToString("0.00", CultureInfo.InvariantCulture)
            : normalizedAmount;
    }

    private static string Get(ParsedFileRecord record, string key)
    {
        return record.Fields.TryGetValue(key, out var value) ? value : string.Empty;
    }

    private void ValidateCardCodeDictionary(
        IReadOnlyCollection<ParsedFileRecord> detailRecords,
        string fileName)
    {
        var fields = new[] { "TxnInstallType", "FinancialType", "TxnEffect", "TxnSource", "TxnRegion", "TerminalType", "IsTxnSettle", "TxnStat", "IsSuccessfulTxn", "TxnOrigin" };
        ValidateCodeDictionaryValues(detailRecords, fields, fileName, "CardTransaction", ResolveCardFieldCodes);
    }

    private void ValidateClearingCodeDictionary(
        IReadOnlyCollection<ParsedFileRecord> detailRecords,
        string fileName,
        ClearingProvider provider)
    {
        var fields = new[] { "TxnType", "IoFlag", "CardDCI", "DisputeCode", "ControlStat" };
        ValidateCodeDictionaryValuesByProvider(detailRecords, fields, fileName, "Clearing", provider, ResolveClearingFieldCodes);
    }

    private static IReadOnlyDictionary<string, string> ResolveCardFieldCodes(string fieldName)
    {
        return FileTypeLookupCodes.CardTransaction.TryGetFieldCodes(fieldName, out var codes)
            ? codes
            : null;
    }

    private static IReadOnlyDictionary<string, string> ResolveClearingFieldCodes(ClearingProvider provider, string fieldName)
    {
        return FileTypeLookupCodes.Clearing.TryGetFieldCodes(provider, fieldName, out var codes)
            ? codes
            : null;
    }

    private void ValidateCodeDictionaryValues(
        IReadOnlyCollection<ParsedFileRecord> detailRecords,
        IReadOnlyCollection<string> fields,
        string fileName,
        string dictionaryName,
        Func<string, IReadOnlyDictionary<string, string>> resolveCodes)
    {
        foreach (var field in fields)
        {
            var codes = resolveCodes(field);
            if (codes is null)
            {
                continue;
            }

            var seenUnknownValues = new HashSet<string>(StringComparer.Ordinal);
            foreach (var detail in detailRecords)
            {
                var raw = Get(detail, field);
                if (string.IsNullOrWhiteSpace(raw))
                {
                    continue;
                }

                var normalized = NormalizeCode(raw);
                if (codes.ContainsKey(normalized) || !seenUnknownValues.Add(normalized))
                {
                    continue;
                }

                _logger.LogWarning(
                    "Unknown code dictionary value detected. File={FileName}, SpecVersion={SpecVersion}, Field={Field}, RawValue={RawValue}, NormalizedValue={NormalizedValue}, LineNo={LineNo}",
                    fileName,
                    dictionaryName,
                    field,
                    raw,
                    normalized,
                    detail.LineNo);
            }
        }
    }

    private void ValidateCodeDictionaryValuesByProvider(
        IReadOnlyCollection<ParsedFileRecord> detailRecords,
        IReadOnlyCollection<string> fields,
        string fileName,
        string dictionaryName,
        ClearingProvider provider,
        Func<ClearingProvider, string, IReadOnlyDictionary<string, string>> resolveCodes)
    {
        foreach (var field in fields)
        {
            var codes = resolveCodes(provider, field);
            if (codes is null)
            {
                continue;
            }

            var seenUnknownValues = new HashSet<string>(StringComparer.Ordinal);
            foreach (var detail in detailRecords)
            {
                var raw = Get(detail, field);
                if (string.IsNullOrWhiteSpace(raw))
                {
                    continue;
                }

                var normalized = NormalizeCode(raw);
                if (codes.ContainsKey(normalized) || !seenUnknownValues.Add(normalized))
                {
                    continue;
                }

                _logger.LogWarning(
                    "Unknown provider code dictionary value detected. File={FileName}, SpecVersion={SpecVersion}, Provider={Provider}, Field={Field}, RawValue={RawValue}, NormalizedValue={NormalizedValue}, LineNo={LineNo}",
                    fileName,
                    dictionaryName,
                    provider,
                    field,
                    raw,
                    normalized,
                    detail.LineNo);
            }
        }
    }

    private static string NormalizeCode(string value)
    {
        return (value ?? string.Empty).Trim().ToUpperInvariant();
    }

    private static string NormalizeKeyField(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim().ToUpperInvariant();
    }

    private static DateOnly? ParseDateYYYYMMDD(string value)
    {
        var normalized = (value ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        return DateOnly.TryParseExact(normalized, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
            ? parsed
            : null;
    }

    private static TimeOnly? ParseTime(string value)
    {
        var normalized = (value ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        if (TimeOnly.TryParseExact(normalized, "HHmmssfff", CultureInfo.InvariantCulture, DateTimeStyles.None, out var withMillis))
        {
            return withMillis;
        }

        if (TimeOnly.TryParseExact(normalized, "HHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var withoutMillis))
        {
            return withoutMillis;
        }

        return null;
    }

    private static int? ParseInt(string value)
    {
        var normalized = (value ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        return int.TryParse(normalized, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }

    private static decimal? ParseImplied2Decimal(string value)
    {
        var normalized = (value ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        if (normalized.All(char.IsDigit) && long.TryParse(normalized, NumberStyles.Integer, CultureInfo.InvariantCulture, out var raw))
        {
            return raw / 100m;
        }

        if (decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out var direct))
        {
            return direct;
        }

        if (decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.GetCultureInfo("tr-TR"), out direct))
        {
            return direct;
        }

        return null;
    }

    private static int? ParseTxnOrigin(string value)
    {
        return ParseInt(value);
    }

    private static void PopulateIncomingFileMetadata(ImportedFile importedFile, IReadOnlyCollection<ParsedFileRecord> parsedRecords)
    {
        var header = parsedRecords.FirstOrDefault(x => x.RecordType == FixedWidthRecordTypes.Header);
        if (header is not null)
        {
            importedFile.HeaderCode = Get(header, "HeaderCode");
            importedFile.DeclaredFileDate = Get(header, "FileDate");
            importedFile.DeclaredFileNo = Get(header, "FileNo");
            importedFile.DeclaredFileVersionNumber = Get(header, "FileVersionNumber");
        }

        var footer = parsedRecords.FirstOrDefault(x => x.RecordType == FixedWidthRecordTypes.Footer);
        if (footer is not null)
        {
            importedFile.FooterCode = Get(footer, "FooterCode");
            importedFile.DeclaredTxnCount = Get(footer, "TxnCount");
        }
    }

    private static (string DeclaredFileDate, string DeclaredFileNo, string DeclaredFileVersionNumber) ExtractDeclaredIdentity(
        IReadOnlyCollection<ParsedFileRecord> parsedRecords)
    {
        var header = parsedRecords.FirstOrDefault(x => x.RecordType == FixedWidthRecordTypes.Header);
        if (header is null)
        {
            return (string.Empty, string.Empty, string.Empty);
        }

        return (Get(header, "FileDate"), Get(header, "FileNo"), Get(header, "FileVersionNumber"));
    }

    private static ClearingProvider ResolveProvider(string fileName)
    {
        if (fileName.Contains(FileNameMarkers.Visa, StringComparison.OrdinalIgnoreCase)) return ClearingProvider.Visa;
        if (fileName.Contains(FileNameMarkers.Msc, StringComparison.OrdinalIgnoreCase) || fileName.Contains(FileNameMarkers.Master, StringComparison.OrdinalIgnoreCase)) return ClearingProvider.Mastercard;
        if (fileName.Contains(FileNameMarkers.Bkm, StringComparison.OrdinalIgnoreCase)) return ClearingProvider.Bkm;
        return ClearingProvider.Unknown;
    }

    private bool MatchesAnyFileNamePattern(string fileName, IReadOnlyCollection<string> patterns)
    {
        if (string.IsNullOrWhiteSpace(fileName) || patterns is null || patterns.Count == 0)
        {
            return false;
        }

        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        if (string.IsNullOrWhiteSpace(fileNameWithoutExtension))
        {
            return false;
        }

        foreach (var pattern in patterns)
        {
            if (string.IsNullOrWhiteSpace(pattern))
            {
                continue;
            }

            try
            {
                if (Regex.IsMatch(fileNameWithoutExtension, pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                {
                    return true;
                }
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid file name pattern configured. Pattern={Pattern}", pattern);
            }
        }

        return false;
    }

    private IReadOnlyCollection<ClearingFtpSource> ResolveClearingFtpSources()
    {
        var sources = new List<ClearingFtpSource>();
        var dateFolder = DateTime.Now.ToString("yyMMdd", CultureInfo.InvariantCulture);

        AddProviderSource(
            sources,
            _settings.Ftp.Source.BkmClearingOutgoingPath,
            dateFolder);
        AddProviderSource(
            sources,
            _settings.Ftp.Source.MastercardClearingOutgoingPath,
            dateFolder);
        AddProviderSource(
            sources,
            _settings.Ftp.Source.VisaClearingOutgoingPath,
            dateFolder);

        return sources
            .Where(x => !string.IsNullOrWhiteSpace(x.IncomingPath))
            .GroupBy(x => x.IncomingPath, StringComparer.OrdinalIgnoreCase)
            .Select(x => x.First())
            .ToArray();
    }

    private void AddProviderSource(
        ICollection<ClearingFtpSource> sources,
        string incomingRootPath,
        string dateFolder)
    {
        if (string.IsNullOrWhiteSpace(incomingRootPath))
        {
            return;
        }

        var incomingPath = CombineRemotePath(incomingRootPath, dateFolder);
        sources.Add(new ClearingFtpSource(incomingPath));
    }

    private static string CombineRemotePath(string rootPath, string child)
    {
        var normalizedRoot = (rootPath ?? string.Empty).Trim().TrimEnd('/', '\\');
        var normalizedChild = (child ?? string.Empty).Trim().Trim('/', '\\');
        if (string.IsNullOrWhiteSpace(normalizedRoot))
        {
            return normalizedChild;
        }

        if (string.IsNullOrWhiteSpace(normalizedChild))
        {
            return normalizedRoot;
        }

        return $"{normalizedRoot}/{normalizedChild}";
    }

    private sealed record ClearingFtpSource(string IncomingPath);

    private static string ComputeContentHash(string content)
    {
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(hash);
    }

    private static string NormalizeFileContent(string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return string.Empty;
        }

        return content
            .Replace("\uFEFF", string.Empty)
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n');
    }

    private static string[] SplitLines(string normalizedContent)
    {
        return normalizedContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
    }
}

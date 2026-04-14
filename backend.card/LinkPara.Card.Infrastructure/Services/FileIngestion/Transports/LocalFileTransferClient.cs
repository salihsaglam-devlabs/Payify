using LinkPara.Card.Application.Commons.Exceptions;
using LinkPara.Card.Application.Commons.Interfaces;
using Microsoft.Extensions.Localization;
using LinkPara.Card.Domain.Enums.FileIngestion;
using Microsoft.Extensions.Options;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using LinkPara.Card.Application.Commons.Extensions;
using LinkPara.Card.Application.Commons.Models.FileIngestion.Configuration;

namespace LinkPara.Card.Infrastructure.Services.FileIngestion.Transports;

public class LocalFileTransferClient : IFileTransferClient
{
    private readonly FileIngestionOptions _options;
    private readonly IStringLocalizer _localizer;
    private readonly ITimeProvider _timeProvider;

    public LocalFileTransferClient(IOptions<FileIngestionOptions> options, ITimeProvider timeProvider, Func<LinkPara.Card.Application.Commons.Localization.LocalizerResource, IStringLocalizer> localizerFactory)
    {
        _options = options.Value;
        _timeProvider = timeProvider;
        _localizer = localizerFactory(LinkPara.Card.Application.Commons.Localization.LocalizerResource.Messages);
    }

    public FileSourceType SourceType => FileSourceType.Local;

    public Task<IReadOnlyCollection<FileReference>> ListAsync(
        string profileKey,
        ProfileOptions profile,
        FileTransferEndpointType endpointType = FileTransferEndpointType.Source,
        CancellationToken cancellationToken = default)
    {
        var localOptions = endpointType == FileTransferEndpointType.Target
            ? _options.Connections.Target.Local
            : _options.Connections.Source.Local;

        if (!localOptions.Paths.TryGetValue(profileKey, out var location))
            return Task.FromResult<IReadOnlyCollection<FileReference>>(Array.Empty<FileReference>());

        var rootPath = ResolveRootPath(location);
        if (string.IsNullOrWhiteSpace(rootPath))
            return Task.FromResult<IReadOnlyCollection<FileReference>>(Array.Empty<FileReference>());

        rootPath = AppendSourceDateSubfolder(rootPath, profile);

        if (File.Exists(rootPath))
        {
            return Task.FromResult<IReadOnlyCollection<FileReference>>(new[]
            {
                new FileReference { Name = Path.GetFileName(rootPath), FullPath = rootPath }
            });
        }

        if (!Directory.Exists(rootPath))
            return Task.FromResult<IReadOnlyCollection<FileReference>>(Array.Empty<FileReference>());

        var regex = new Regex(profile.Pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        var files = Directory.EnumerateFiles(rootPath)
            .Where(path => profile.FileExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase))
            .Where(path => regex.IsMatch(Path.GetFileNameWithoutExtension(path)))
            .OrderBy(path => Path.GetFileName(path), FileReferenceNameComparer.Instance)
            .Select(path => new FileReference
            {
                Name = Path.GetFileName(path),
                FullPath = path
            })
            .ToArray();

        return Task.FromResult<IReadOnlyCollection<FileReference>>(files);
    }

    public Task<Stream> OpenReadAsync(FileReference file, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Stream stream = File.Open(file.FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        return Task.FromResult(stream);
    }

    public Task<byte[]> ReadAllBytesAsync(FileReference file, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return File.ReadAllBytesAsync(file.FullPath, cancellationToken);
    }

    public async Task<string> ReadRangeAsync(
        FileReference file,
        long byteOffset,
        int byteLength,
        Encoding encoding,
        CancellationToken cancellationToken = default)
    {
        await using var stream = File.Open(file.FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        stream.Seek(byteOffset, SeekOrigin.Begin);
        var buffer = new byte[byteLength];
        var totalRead = 0;

        while (totalRead < byteLength)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(totalRead, byteLength - totalRead), cancellationToken);
            if (read == 0)
                break;

            totalRead += read;
        }

        return encoding.GetString(buffer, 0, totalRead);
    }

    public Task<Stream> OpenWriteAsync(
        string profileKey,
        string fileName,
        FileTransferEndpointType endpointType = FileTransferEndpointType.Target,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var fullPath = BuildTargetFilePath(profileKey, fileName, endpointType);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        Stream stream = File.Open(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        return Task.FromResult(stream);
    }

    public async Task<FileReference> WriteAllBytesAsync(
        string profileKey,
        string fileName,
        ReadOnlyMemory<byte> content,
        FileTransferEndpointType endpointType = FileTransferEndpointType.Target,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var fullPath = BuildTargetFilePath(profileKey, fileName, endpointType);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        await File.WriteAllBytesAsync(fullPath, content.ToArray(), cancellationToken);
        return new FileReference { Name = fileName, FullPath = fullPath };
    }

    private string BuildTargetFilePath(string profileKey, string fileName, FileTransferEndpointType endpointType)
    {
        var localOptions = endpointType == FileTransferEndpointType.Target
            ? _options.Connections.Target.Local
            : _options.Connections.Source.Local;

        if (!localOptions.Paths.TryGetValue(profileKey, out var location))
            throw new FileIngestionLocalPathNotConfiguredException(_localizer.Get("FileIngestion.LocalPathNotConfigured", profileKey));

        var rootPath = ResolveRootPath(location);
        if (string.IsNullOrWhiteSpace(rootPath))
            throw new FileIngestionLocalRootPathEmptyException(_localizer.Get("FileIngestion.LocalRootPathEmpty", profileKey));

        var now = _timeProvider.Now;
        var dateFolder = now.ToString("yyyy-MM-dd");
        var timeFolder = now.ToString("HH-mm-ss-fff");
        var timestampPath = Path.Combine(rootPath, dateFolder, timeFolder);

        return Path.Combine(timestampPath, fileName);
    }

    private static string ResolveRootPath(PathOptions location)
    {
        if (!string.IsNullOrWhiteSpace(location.Current))
            return location.Current;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return location.Defaults.Windows;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return location.Defaults.MacOS;

        return location.Defaults.Linux;
    }

    private string AppendSourceDateSubfolder(string rootPath, ProfileOptions profile)
    {
        if (string.IsNullOrWhiteSpace(profile.SourceDateSubfolderFormat))
            return rootPath;

        var dateSubfolder = _timeProvider.Now.ToString(profile.SourceDateSubfolderFormat);
        return Path.Combine(rootPath, dateSubfolder);
    }
}

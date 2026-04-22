using Microsoft.Extensions.Localization;
using LinkPara.Card.Application.Commons.Interfaces.FileIngestion;
using LinkPara.Card.Application.Commons.Exceptions;
using LinkPara.Card.Domain.Enums.FileIngestion;
using LinkPara.Card.Infrastructure.Persistence;
using LinkPara.Card.Infrastructure.Services.Audit;
using LinkPara.Card.Infrastructure.Services.FileIngestion.Parsing;
using LinkPara.Card.Infrastructure.Services.FileIngestion.Transports;
using LinkPara.SharedModels.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using NpgsqlTypes;
using System.Buffers;
using System.Collections.Concurrent;
using System.Data;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using LinkPara.Card.Application.Commons.Extensions;
using LinkPara.Card.Application.Commons.Helpers;
using LinkPara.Card.Application.Commons.Models.AppConfiguration;
using LinkPara.Card.Application.Commons.Models.FileIngestion.Configuration;
using LinkPara.Card.Application.Commons.Models.FileIngestion.Contracts.Requests;
using LinkPara.Card.Application.Commons.Models.FileIngestion.Contracts.Responses;
using LinkPara.SharedModels.Exceptions;
using IngestionFileLineEntity = LinkPara.Card.Domain.Entities.FileIngestion.Persistence.IngestionFileLine;
using IngestionFileEntity = LinkPara.Card.Domain.Entities.FileIngestion.Persistence.IngestionFile;
using LinkPara.Card.Domain.Entities.FileIngestion.Persistence;
using LinkPara.Card.Infrastructure.Services.Reconciliation.Evaluate.Core;

namespace LinkPara.Card.Infrastructure.Services.FileIngestion.Orchestration;

public class FileIngestionOrchestrator : IFileIngestionService
{
    private const int QueryBatchSize = 10_000;
    private const int BulkCopyInternalBatchSize = 10_000;
    private const int BulkCopyTimeoutSeconds = 600;
    private const int MaxRetryErrorsPerAttempt = 1000;
    private static readonly ConcurrentDictionary<string, Regex> _profileRegexCache = new();
    private static readonly ConcurrentDictionary<(Type EntityType, string PropertyName), Func<object, object?>>
        _propertyGetterCache = new();
    private static readonly ConcurrentDictionary<Type, Func<object, string>> _enumToStringCache = new();
    private static readonly JsonSerializerOptions _parsedContentJsonOptions = new();
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<ProfileOptions, Regex>
        _profileRegexByInstance = new();
    private static readonly ConcurrentDictionary<Type, IReadOnlyList<IProperty>> _ingestionBulkPropertyCache = new();
    private static (string Schema, string LineTable, string FileTable)? _cachedLineTableNames;
    private static (Dictionary<string, string> LineCols, Dictionary<string, string> FileCols)? _cachedColumnNames;
    private static readonly object _modelCacheLock = new();

    private readonly CardDbContext _dbContext;
    private readonly IAuditStampService _auditStampService;
    private readonly IFileTransferClientResolver _fileTransferClientResolver;
    private readonly IFixedWidthRecordParser _fixedWidthRecordParser;
    private readonly IParsedRecordModelMapper _parsedRecordModelMapper;
    private readonly IIngestionDetailEntityMapper _detailEntityMapper;
    private readonly CardConfigOptions.IngestEndpoint _options;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IIngestionErrorMapper _ingestionErrorMapper;
    private readonly ILogger<FileIngestionOrchestrator> _logger;
    private readonly IStringLocalizer _localizer;
    private readonly IClearingArrivalRequeueService _clearingArrivalRequeueService;

    public FileIngestionOrchestrator(
        CardDbContext dbContext,
        IAuditStampService auditStampService,
        IFileTransferClientResolver fileTransferClientResolver,
        IFixedWidthRecordParser fixedWidthRecordParser,
        IParsedRecordModelMapper parsedRecordModelMapper,
        IIngestionDetailEntityMapper detailEntityMapper,
        IOptions<CardConfigOptions> options,
        IServiceScopeFactory serviceScopeFactory,
        IIngestionErrorMapper ingestionErrorMapper,
        ILogger<FileIngestionOrchestrator> logger,
        Func<LinkPara.Card.Application.Commons.Localization.LocalizerResource, IStringLocalizer> localizerFactory,
        IClearingArrivalRequeueService clearingArrivalRequeueService)
    {
        _dbContext = dbContext;
        _auditStampService = auditStampService;
        _fileTransferClientResolver = fileTransferClientResolver;
        _fixedWidthRecordParser = fixedWidthRecordParser;
        _parsedRecordModelMapper = parsedRecordModelMapper;
        _detailEntityMapper = detailEntityMapper;
        _options = options.Value.Endpoints.FileIngestion.Ingest;
        _serviceScopeFactory = serviceScopeFactory;
        _ingestionErrorMapper = ingestionErrorMapper;
        _logger = logger;
        _localizer = localizerFactory(LinkPara.Card.Application.Commons.Localization.LocalizerResource.Messages);
        _clearingArrivalRequeueService = clearingArrivalRequeueService;
    }

    public async Task<List<FileIngestionResponse>> IngestAsync(
        FileIngestionRequest request,
        CancellationToken cancellationToken = default)
    {
        var globalErrors = new List<IngestionErrorDetail>();

        try
        {
            if (request == null)
            {
                var error = new IngestionErrorDetail
                {
                    Code = "INVALID_REQUEST",
                    Message = _localizer.Get("FileIngestion.RequestIsNull"),
                    Step = "VALIDATION",
                    Severity = "Error"
                };
                globalErrors.Add(error);
                return new List<FileIngestionResponse> { CreateErrorResponse("unknown", globalErrors) };
            }

            string profileKey;
            ProfileOptions profile;
            ParsingOptions parsingRule;
            IFileTransferClient transferClient;

            try
            {
                profileKey = BuildProfileKey(request.FileType, request.FileContentType);
                profile = GetProfile(profileKey);
                parsingRule = GetParsingRule(profile);
                transferClient = _fileTransferClientResolver.Create(request.FileSourceType);
            }
            catch (Exception ex)
            {
                var error = _ingestionErrorMapper.MapException(ex, "CONFIGURATION", fileName: request.FilePath);
                error.Step = "CONFIGURATION";
                globalErrors.Add(error);
                return new List<FileIngestionResponse>
                    { CreateErrorResponse(request.FilePath ?? "unknown", globalErrors) };
            }

            IReadOnlyCollection<FileReference> files;
            try
            {
                files = await ResolveFilesAsync(request, profileKey, profile, transferClient, cancellationToken);
            }
            catch (Exception ex)
            {
                var error = _ingestionErrorMapper.MapIOError(ex, request.FilePath ?? "unknown");
                error.Step = "FILE_RESOLUTION";
                globalErrors.Add(error);
                return new List<FileIngestionResponse>
                    { CreateErrorResponse(error.FileName ?? request.FilePath ?? "unknown", globalErrors) };
            }

            if (files.Count == 0)
            {
                var error = new IngestionErrorDetail
                {
                    Code = "FILE_NOT_FOUND",
                    Message = _localizer.Get("FileIngestion.NoFileMatchedProfile", profileKey),
                    Detail =
                        $"Profile: {profileKey}, FileSourceType: {request.FileSourceType}, FilePath: {request.FilePath}",
                    Step = "FILE_RESOLUTION",
                    FileName = request.FilePath ?? "unknown",
                    Severity = "Error"
                };
                globalErrors.Add(error);
                return new List<FileIngestionResponse>
                    { CreateErrorResponse(request.FilePath ?? "unknown", globalErrors) };
            }

            var fileList = files.ToList();
            var responsesArray = new FileIngestionResponse[fileList.Count];
            var tasks = new List<Task>();

            if (_options.Processing.EnableParallelProcessing == true && fileList.Count > 1)
            {
                var maxDop = GetMaxDegreeOfParallelism();
                var semaphore = new SemaphoreSlim(maxDop);

                for (int i = 0; i < fileList.Count; i++)
                {
                    int idx = i;
                    var file = fileList[idx];

                    tasks.Add(Task.Run(async () =>
                    {
                        await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                        try
                        {
                            var fileErrors = new List<IngestionErrorDetail>();
                            IngestionFileEntity? fileEntity = null;

                            try
                            {
                                using var scope = _serviceScopeFactory.CreateScope();
                                var scopedOrchestrator =
                                    (FileIngestionOrchestrator)scope.ServiceProvider
                                        .GetRequiredService<IFileIngestionService>();

                                fileEntity = await scopedOrchestrator.ImportSingleFileAsync(
                                    file,
                                    request.FileSourceType,
                                    request.FileType,
                                    request.FileContentType,
                                    profile,
                                    parsingRule,
                                    transferClient,
                                    fileErrors,
                                    cancellationToken).ConfigureAwait(false);
                            }
                            catch (OperationCanceledException ex)
                            {
                                if (cancellationToken.IsCancellationRequested)
                                    throw;

                                fileErrors.Add(new IngestionErrorDetail
                                {
                                    Code = "OPERATION_CANCELLED",
                                    Message = _localizer.Get("FileIngestion.Cancelled"),
                                    Detail = ExceptionDetailHelper.BuildDetailMessage(ex),
                                    Step = "IMPORT",
                                    FileName = file.Name,
                                    Severity = "Warning"
                                });
                            }
                            catch (Exception ex)
                            {
                                fileErrors.Add(_ingestionErrorMapper.MapException(ex, "IMPORT", fileName: file.Name));
                            }

                            FileIngestionResponse resp;

                            if (fileEntity == null && fileErrors.Count == 0)
                            {
                                var e = new IngestionErrorDetail
                                {
                                    Code = "IMPORT_FAILED",
                                    Message = _localizer.Get("FileIngestion.ImportFailedNoDetails"),
                                    Step = "IMPORT",
                                    Severity = "Error",
                                    FileName = file.Name
                                };

                                fileErrors.Add(e);
                                resp = CreateErrorResponse(file.Name, fileErrors);
                            }
                            else if (fileEntity == null)
                            {
                                resp = CreateErrorResponse(file.Name, fileErrors);
                            }
                            else
                            {
                                resp = ToResponse(fileEntity);
                                resp.Errors = fileErrors;
                            }

                            responsesArray[idx] = resp;
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }, CancellationToken.None));
                }

                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            else
            {
                for (int i = 0; i < fileList.Count; i++)
                {
                    var file = fileList[i];
                    var fileErrors = new List<IngestionErrorDetail>();
                    IngestionFileEntity? fileEntity = null;

                    try
                    {
                        fileEntity = await ImportSingleFileAsync(
                            file,
                            request.FileSourceType,
                            request.FileType,
                            request.FileContentType,
                            profile,
                            parsingRule,
                            transferClient,
                            fileErrors,
                            cancellationToken).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException ex)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            throw;

                        fileErrors.Add(new IngestionErrorDetail
                        {
                            Code = "OPERATION_CANCELLED",
                            Message = _localizer.Get("FileIngestion.Cancelled"),
                            Detail = ExceptionDetailHelper.BuildDetailMessage(ex),
                            Step = "IMPORT",
                            FileName = file.Name,
                            Severity = "Warning"
                        });
                    }
                    catch (Exception ex)
                    {
                        fileErrors.Add(_ingestionErrorMapper.MapException(ex, "IMPORT", fileName: file.Name));
                    }

                    FileIngestionResponse resp;

                    if (fileEntity == null && fileErrors.Count == 0)
                    {
                        var e = new IngestionErrorDetail
                        {
                            Code = "IMPORT_FAILED",
                            Message = _localizer.Get("FileIngestion.ImportFailedNoDetails"),
                            Step = "IMPORT",
                            Severity = "Error",
                            FileName = file.Name
                        };

                        fileErrors.Add(e);
                        resp = CreateErrorResponse(file.Name, fileErrors);
                    }
                    else if (fileEntity == null)
                    {
                        resp = CreateErrorResponse(file.Name, fileErrors);
                    }
                    else
                    {
                        resp = ToResponse(fileEntity);
                        resp.Errors = fileErrors;
                    }

                    responsesArray[i] = resp;
                }
            }

            return responsesArray.Where(r => r != null).ToList();
        }
        catch (OperationCanceledException ex)
        {
            var error = new IngestionErrorDetail
            {
                Code = "OPERATION_CANCELLED",
                Message = _localizer.Get("FileIngestion.OperationCancelled"),
                Detail = ExceptionDetailHelper.BuildDetailMessage(ex),
                Step = "INGESTION",
                Severity = "Warning"
            };

            return new List<FileIngestionResponse>
                { CreateErrorResponse(request?.FilePath ?? "unknown", new List<IngestionErrorDetail> { error }) };
        }
        catch (Exception ex)
        {
            var error = _ingestionErrorMapper.MapException(ex, "INGESTION", fileName: request?.FilePath);
            return new List<FileIngestionResponse>
                { CreateErrorResponse(request?.FilePath ?? "unknown", new List<IngestionErrorDetail> { error }) };
        }
    }

    public async Task<IngestionFileEntity> ImportSingleFileAsync(
        FileReference file,
        FileSourceType sourceType,
        FileType fileType,
        FileContentType fileContentType,
        ProfileOptions profile,
        ParsingOptions parsingRule,
        IFileTransferClient transferClient,
        List<IngestionErrorDetail> errors,
        CancellationToken cancellationToken)
    {
        var encoding = ResolveEncoding(profile.DefaultEncoding);
        var profileKey = BuildProfileKey(fileType, fileContentType);

        BoundaryRecord headerRecord;
        BoundaryRecord footerRecord;
        try
        {
            (headerRecord, footerRecord) = await ReadBoundaryRecordsAsync(
                file,
                profileKey,
                parsingRule,
                transferClient,
                encoding,
                cancellationToken);
        }
        catch (Exception ex)
        {
            var error = _ingestionErrorMapper.MapException(ex, "BOUNDARY_READ", fileName: file.Name);
            error.Code = ResolveBoundaryReadErrorCode(ex);
            error.Detail = _localizer.Get("FileIngestion.Detail.BoundaryReadFailed",
                ExceptionDetailHelper.BuildDetailMessage(ex));
            errors.Add(error);
            throw;
        }

        string fileKey;
        try
        {
            fileKey = GenerateFileKey(headerRecord.TypedModel, footerRecord.TypedModel);
        }
        catch (Exception ex)
        {
            var error = _ingestionErrorMapper.MapException(ex, "FILE_KEY_GENERATION", fileName: file.Name);
            error.Detail = _localizer.Get("FileIngestion.Detail.FileKeyGenerationFailed");
            errors.Add(error);
            throw;
        }

        IngestionFileEntity? existing;
        try
        {
            existing = await _dbContext.IngestionFiles
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                        x.FileKey == fileKey &&
                        x.FileName == file.Name &&
                        x.SourceType == sourceType &&
                        x.FileType == fileType,
                    cancellationToken);
        }
        catch (Exception ex)
        {
            var error = _ingestionErrorMapper.MapDatabaseError(ex, fileName: file.Name);
            error.Step = "DUPLICATE_CHECK";
            error.Detail = _localizer.Get("FileIngestion.Detail.DuplicateCheckFailed");
            errors.Add(error);
            throw;
        }

        if (existing is not null)
        {
            if (RequiresArchiveOnlyRecovery(existing))
                return await RetryArchiveOnlyAsync(existing.Id, file, transferClient, errors, cancellationToken);

            if (RequiresFullRecovery(existing))
            {
                return await RecoverExistingFileAsync(
                    existing.Id,
                    file,
                    profileKey,
                    profile,
                    parsingRule,
                    transferClient,
                    errors,
                    cancellationToken);
            }

            if (await RequiresReArchiveAsync(existing.Id, cancellationToken))
            {
                try
                {
                    await UpdateFileMessageAsync(
                        existing.Id,
                        _localizer.Get("FileIngestion.ReArchiveStarted"),
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    var error = _ingestionErrorMapper.MapDatabaseError(ex, fileName: file.Name);
                    error.Step = "REARCHIVE_MESSAGE_UPDATE";
                    errors.Add(error);
                }

                return await RetryArchiveOnlyAsync(existing.Id, file, transferClient, errors, cancellationToken);
            }

            try
            {
                await UpdateFileMessageAsync(existing.Id, _localizer.Get("FileIngestion.DuplicateFileReceived"),
                    cancellationToken);
            }
            catch (Exception ex)
            {
                var error = _ingestionErrorMapper.MapDatabaseError(ex, fileName: file.Name);
                error.Step = "DUPLICATE_UPDATE";
                errors.Add(error);
            }

            try
            {
                _dbContext.ChangeTracker.Clear();
                return await GetFileAsync(existing.Id, cancellationToken);
            }
            catch (Exception ex)
            {
                var error = _ingestionErrorMapper.MapDatabaseError(ex, fileName: file.Name);
                error.Step = "DUPLICATE_RETRIEVAL";
                errors.Add(error);
                throw;
            }
        }

        return await ProcessNewFileAsync(
            file,
            sourceType,
            fileType,
            fileContentType,
            profileKey,
            profile,
            parsingRule,
            transferClient,
            footerRecord,
            fileKey,
            errors,
            cancellationToken);
    }

    private async Task<IngestionFileEntity> ProcessNewFileAsync(
        FileReference file,
        FileSourceType sourceType,
        FileType fileType,
        FileContentType fileContentType,
        string profileKey,
        ProfileOptions profile,
        ParsingOptions parsingRule,
        IFileTransferClient transferClient,
        BoundaryRecord footerRecord,
        string fileKey,
        List<IngestionErrorDetail> errors,
        CancellationToken cancellationToken)
    {
        var transactionFile = new IngestionFileEntity
        {
            FileKey = fileKey,
            FileName = file.Name,
            FilePath = file.FullPath,
            SourceType = sourceType,
            FileType = fileType,
            ContentType = fileContentType,
            Status = FileStatus.Processing,
            Message = _localizer.Get("FileIngestion.ImportStarted"),
            ExpectedLineCount = GetFooterTxnCount(footerRecord.TypedModel),
            IsArchived = false
        };

        _auditStampService.StampForCreate(transactionFile);

        try
        {
            _dbContext.IngestionFiles.Add(transactionFile);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            _dbContext.ChangeTracker.Clear();

            var existing = await _dbContext.IngestionFiles
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                        x.FileKey == fileKey &&
                        x.FileName == file.Name &&
                        x.SourceType == sourceType &&
                        x.FileType == fileType,
                    cancellationToken);

            if (existing is not null)
            {
                return await GetFileAsync(existing.Id, cancellationToken);
            }

            var error = _ingestionErrorMapper.MapDatabaseError(ex, fileName: file.Name);
            error.Step = "FILE_ENTITY_CREATE";
            error.Detail = _localizer.Get("FileIngestion.Detail.DuplicateInsertNotResolved");
            errors.Add(error);
            throw;
        }
        catch (Exception ex)
        {
            var error = _ingestionErrorMapper.MapDatabaseError(ex, fileName: file.Name);
            error.Step = "FILE_ENTITY_CREATE";
            error.Detail = _localizer.Get("FileIngestion.Detail.FileEntityCreateFailed");
            errors.Add(error);
            throw;
        }

        var transactionFileId = transactionFile.Id;
        _dbContext.ChangeTracker.Clear();

        TargetWriter targetWriter;
        try
        {
            targetWriter = await TryOpenTargetWriterAsync(profileKey, file.Name, cancellationToken);
        }
        catch (Exception ex)
        {
            var error = _ingestionErrorMapper.MapException(ex, "ARCHIVE_OPEN", fileName: file.Name);
            error.Detail = _localizer.Get("FileIngestion.Detail.ArchiveOpenFailed");
            errors.Add(error);
            throw;
        }

        var batch = new List<IngestionFileLineEntity>(GetBatchSize());

        Stream stream;
        try
        {
            stream = await transferClient.OpenReadAsync(file, cancellationToken);
        }
        catch (Exception ex)
        {
            var error = _ingestionErrorMapper.MapIOError(ex, fileName: file.Name);
            error.Step = "FILE_STREAM_OPEN";
            error.Detail = _localizer.Get("FileIngestion.Detail.FileStreamOpenFailed");
            errors.Add(error);
            throw;
        }

        var progress = new ProcessingProgress();

        try
        {
            await using (stream)
            {
                await ReadLinesWithByteOffsetsAsync(
                    stream,
                    ResolveEncoding(profile.DefaultEncoding),
                    async lineReadResult =>
                    {
                        var row = BuildIngestionFileLine(
                            transactionFileId,
                            profileKey,
                            fileType,
                            lineReadResult,
                            parsingRule);

                        batch.Add(row);
                        progress.LastProcessedLineNumber = lineReadResult.LineNumber;
                        progress.LastProcessedByteOffset = lineReadResult.NextByteOffset;

                        if (row.LineType == "D")
                        {
                            progress.TotalCount++;
                            if (row.Status == FileRowStatus.Success)
                                progress.SuccessCount++;
                            else
                                progress.ErrorCount++;
                        }

                        if (batch.Count < GetBatchSize())
                            return;

                        await PersistBatchAsync(transactionFileId, batch, progress, cancellationToken);
                    },
                    targetWriter.WriteAsync,
                    cancellationToken);

                if (batch.Count > 0)
                    await PersistBatchAsync(transactionFileId, batch, progress, cancellationToken,
                        forceCheckpoint: true);
            }
        }
        catch (Exception ex)
        {
            var error = _ingestionErrorMapper.MapException(ex, "FILE_PROCESSING", fileName: file.Name);
            error.Detail = _localizer.Get("FileIngestion.Detail.FileProcessingError", progress.LastProcessedLineNumber,
                ExceptionDetailHelper.BuildDetailMessage(ex));
            if (progress.LastProcessedLineNumber > 0)
                error.LineNumber = progress.LastProcessedLineNumber;
            errors.Add(error);
            throw;
        }

        try
        {
            await targetWriter.FinalizeAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            var error = _ingestionErrorMapper.MapException(ex, "ARCHIVE_FINALIZE", fileName: file.Name);
            error.Detail = _localizer.Get("FileIngestion.Detail.ArchiveFinalizeFailed");
            if (!string.IsNullOrEmpty(targetWriter.ErrorMessage))
                error.Detail += _localizer.Get("FileIngestion.Detail.ArchiveError", targetWriter.ErrorMessage);
            errors.Add(error);
            throw;
        }

        if (!string.IsNullOrWhiteSpace(targetWriter.ErrorMessage))
        {
            errors.Add(new IngestionErrorDetail
            {
                Code = "ARCHIVE_FAILED",
                Message = _localizer.Get("FileIngestion.FileImportedNotArchived", targetWriter.ErrorMessage),
                Detail = targetWriter.ErrorMessage,
                Step = targetWriter.ErrorStep ?? "ARCHIVE",
                FileName = file.Name,
                Severity = "Error"
            });
        }

        try
        {
            var auditStamp = _auditStampService.CreateStamp();

            var isArchived = string.IsNullOrWhiteSpace(targetWriter.ErrorMessage);

            var affectedRows = await _dbContext.IngestionFiles
                .Where(x => x.Id == transactionFileId)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(x => x.IsArchived, isArchived)
                        .SetProperty(x => x.UpdateDate, auditStamp.Timestamp)
                        .SetProperty(x => x.LastModifiedBy, auditStamp.UserId),
                    cancellationToken);

            if (affectedRows != 1)
            {
                throw new FileIngestionArchiveStatusUpdateRowMismatchException(
                    _localizer.Get("FileIngestion.ArchiveStatusUpdateRowMismatch", affectedRows, transactionFileId));
            }

            _dbContext.ChangeTracker.Clear();
        }
        catch (Exception ex)
        {
            var error = _ingestionErrorMapper.MapDatabaseError(ex, fileName: file.Name);
            error.Step = "ARCHIVE_MARK_COMPLETE";
            error.Detail = _localizer.Get("FileIngestion.Detail.ArchiveStatusUpdateFailed");
            errors.Add(error);
            throw;
        }

        try
        {
            await FinalizeFileStateAsync(transactionFileId, targetWriter.ErrorMessage, cancellationToken);
        }
        catch (Exception ex)
        {
            var error = _ingestionErrorMapper.MapDatabaseError(ex, fileName: file.Name);
            error.Step = "FILE_FINALIZE";
            error.Detail = _localizer.Get("FileIngestion.Detail.FileFinalizeFailed");
            errors.Add(error);
            throw;
        }

        try
        {
            _dbContext.ChangeTracker.Clear();
            return await GetFileAsync(transactionFileId, cancellationToken);
        }
        catch (Exception ex)
        {
            var error = _ingestionErrorMapper.MapDatabaseError(ex, fileName: file.Name);
            error.Step = "FILE_RETRIEVAL";
            error.Detail = _localizer.Get("FileIngestion.Detail.FileRetrievalFailed");
            errors.Add(error);
            throw;
        }
    }

    private async Task<IngestionFileEntity> RecoverExistingFileAsync(
        Guid transactionFileId,
        FileReference sourceFile,
        string profileKey,
        ProfileOptions profile,
        ParsingOptions parsingRule,
        IFileTransferClient sourceTransferClient,
        List<IngestionErrorDetail> errors,
        CancellationToken cancellationToken)
    {
        IngestionFileEntity file;
        try
        {
            _dbContext.ChangeTracker.Clear();
            file = await GetFileAsync(transactionFileId, cancellationToken);
        }
        catch (Exception ex)
        {
            var error = _ingestionErrorMapper.MapDatabaseError(ex, fileName: sourceFile.Name);
            error.Step = "RECOVERY_INIT";
            error.Detail = _localizer.Get("FileIngestion.Detail.RecoveryInitFailed");
            errors.Add(error);
            throw;
        }

        try
        {
            await ContinueMissingRowsAsync(
                file,
                sourceFile,
                profileKey,
                profile,
                parsingRule,
                sourceTransferClient,
                errors,
                cancellationToken);
        }
        catch (Exception ex)
        {
            var error = _ingestionErrorMapper.MapException(ex, "RECOVERY_MISSING_ROWS", fileName: sourceFile.Name);
            error.Detail = _localizer.Get("FileIngestion.Detail.RecoveryMissingRowsFailed");
            errors.Add(error);
        }

        try
        {
            await RetryFailedRowsAsync(
                transactionFileId,
                sourceFile,
                profileKey,
                profile,
                parsingRule,
                sourceTransferClient,
                errors,
                cancellationToken);
        }
        catch (Exception ex)
        {
            var error = _ingestionErrorMapper.MapException(ex, "RECOVERY_FAILED_ROWS", fileName: sourceFile.Name);
            error.Detail = _localizer.Get("FileIngestion.Detail.RecoveryFailedRowsFailed");
            errors.Add(error);
        }

        try
        {
            _dbContext.ChangeTracker.Clear();
            file = await GetFileAsync(transactionFileId, cancellationToken);
        }
        catch (Exception ex)
        {
            var error = _ingestionErrorMapper.MapDatabaseError(ex, fileName: sourceFile.Name);
            error.Step = "RECOVERY_STATUS_CHECK";
            error.Detail = _localizer.Get("FileIngestion.Detail.RecoveryStatusCheckFailed");
            errors.Add(error);
            throw;
        }

        if (!file.IsArchived || await RequiresReArchiveAsync(transactionFileId, cancellationToken))
        {
            try
            {
                file = await RetryArchiveOnlyAsync(transactionFileId, sourceFile, sourceTransferClient, errors,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                var error = _ingestionErrorMapper.MapException(ex, "RECOVERY_ARCHIVE", fileName: sourceFile.Name);
                error.Detail = _localizer.Get("FileIngestion.Detail.RecoveryArchiveFailed");
                errors.Add(error);
            }
        }
        else
        {
            try
            {
                await FinalizeFileStateAsync(transactionFileId, null, cancellationToken);
            }
            catch (Exception ex)
            {
                var error = _ingestionErrorMapper.MapDatabaseError(ex, fileName: sourceFile.Name);
                error.Step = "RECOVERY_FINALIZE";
                error.Detail = _localizer.Get("FileIngestion.Detail.RecoveryFinalizeFailed");
                errors.Add(error);
            }
        }

        try
        {
            _dbContext.ChangeTracker.Clear();
            return await GetFileAsync(transactionFileId, cancellationToken);
        }
        catch (Exception ex)
        {
            var error = _ingestionErrorMapper.MapDatabaseError(ex, fileName: sourceFile.Name);
            error.Step = "RECOVERY_FINAL_RETRIEVAL";
            error.Detail = _localizer.Get("FileIngestion.Detail.RecoveryFinalRetrievalFailed");
            errors.Add(error);
            throw;
        }
    }

    private async Task ContinueMissingRowsAsync(
        IngestionFileEntity file,
        FileReference sourceFile,
        string profileKey,
        ProfileOptions profile,
        ParsingOptions parsingRule,
        IFileTransferClient sourceTransferClient,
        List<IngestionErrorDetail> errors,
        CancellationToken cancellationToken)
    {
        if (file.ExpectedLineCount <= file.ProcessedLineCount)
            return;

        var batch = new List<IngestionFileLineEntity>(GetBatchSize());
        var progress = new ProcessingProgress
        {
            TotalCount = file.ProcessedLineCount,
            SuccessCount = file.SuccessfulLineCount,
            ErrorCount = file.FailedLineCount,
            LastProcessedLineNumber = file.LastProcessedLineNumber,
            LastProcessedByteOffset = file.LastProcessedByteOffset
        };

        Stream stream;
        try
        {
            stream = await sourceTransferClient.OpenReadAsync(sourceFile, cancellationToken);
        }
        catch (Exception ex)
        {
            var error = _ingestionErrorMapper.MapIOError(ex, fileName: sourceFile.Name);
            error.Step = "RECOVERY_STREAM_OPEN";
            error.Detail = _localizer.Get("FileIngestion.Detail.RecoveryStreamOpenFailed");
            errors.Add(error);
            throw;
        }

        try
        {
            if (file.LastProcessedByteOffset > 0)
                await SkipToOffsetAsync(stream, file.LastProcessedByteOffset, cancellationToken);

            await ReadLinesWithByteOffsetsAsync(
                stream,
                ResolveEncoding(profile.DefaultEncoding),
                async lineReadResult =>
                {
                    var row = BuildIngestionFileLine(
                        file.Id,
                        profileKey,
                        file.FileType,
                        lineReadResult,
                        parsingRule);

                    batch.Add(row);
                    progress.LastProcessedLineNumber = lineReadResult.LineNumber;
                    progress.LastProcessedByteOffset = lineReadResult.NextByteOffset;

                    if (row.LineType == "D")
                    {
                        progress.TotalCount++;
                        if (row.Status == FileRowStatus.Success)
                            progress.SuccessCount++;
                        else
                            progress.ErrorCount++;
                    }

                    if (batch.Count < GetBatchSize())
                        return;

                    await PersistBatchAsync(file.Id, batch, progress, cancellationToken);
                },
                null,
                cancellationToken,
                file.LastProcessedLineNumber,
                file.LastProcessedByteOffset);

            if (batch.Count > 0)
                await PersistBatchAsync(file.Id, batch, progress, cancellationToken, forceCheckpoint: true);
        }
        catch (Exception ex)
        {
            var error = _ingestionErrorMapper.MapException(ex, "RECOVERY_MISSING_ROWS", fileName: sourceFile.Name);
            error.Detail = _localizer.Get("FileIngestion.Detail.RecoveryMissingRowsProcessingError",
                progress.LastProcessedLineNumber, ExceptionDetailHelper.BuildDetailMessage(ex));
            if (progress.LastProcessedLineNumber > 0)
                error.LineNumber = progress.LastProcessedLineNumber;
            errors.Add(error);
            throw;
        }
    }

    private async Task RetryFailedRowsAsync(
        Guid transactionFileId,
        FileReference sourceFile,
        string profileKey,
        ProfileOptions profile,
        ParsingOptions parsingRule,
        IFileTransferClient sourceTransferClient,
        List<IngestionErrorDetail> errors,
        CancellationToken cancellationToken)
    {
        try
        {
            Stream? sharedStream = null;
            try
            {
                sharedStream = await sourceTransferClient.OpenReadAsync(sourceFile, cancellationToken);
                if (!sharedStream.CanSeek)
                {
                    await sharedStream.DisposeAsync();
                    sharedStream = null;
                }
            }
            catch
            {
                sharedStream = null;
            }

            var encoding = ResolveEncoding(profile.DefaultEncoding);

            try
            {
                while (true)
                {
                    var failedRows = await _dbContext.IngestionFileLines
                        .AsTracking()
                        .Where(x => x.FileId == transactionFileId && x.Status == FileRowStatus.Failed)
                        .Where(x => x.RetryCount < GetFailedRowMaxRetryCount())
                        .OrderBy(x => x.LineNumber)
                        .Take(GetRetryBatchSize())
                        .ToListAsync(cancellationToken);

                    if (failedRows.Count == 0)
                        break;

                    foreach (var row in failedRows)
                    {
                        try
                        {
                            if (row.ReconciliationStatus == ReconciliationStatus.Success)
                                continue;

                            string rawLine;
                            if (sharedStream is not null)
                            {
                                try
                                {
                                    rawLine = await ReadRangeFromStreamAsync(
                                        sharedStream, row.ByteOffset, row.ByteLength, encoding, cancellationToken);
                                }
                                catch
                                {
                                    try
                                    {
                                        await sharedStream.DisposeAsync();
                                    }
                                    catch
                                    {
                                        /* ignore */
                                    }

                                    sharedStream = null;
                                    rawLine = await sourceTransferClient.ReadRangeAsync(
                                        sourceFile, row.ByteOffset, row.ByteLength, encoding, cancellationToken);
                                }
                            }
                            else
                            {
                                rawLine = await sourceTransferClient.ReadRangeAsync(
                                    sourceFile, row.ByteOffset, row.ByteLength, encoding, cancellationToken);
                            }

                            var parsed = _fixedWidthRecordParser.Parse(rawLine, parsingRule);
                            parsed.ParsedDataModel = _parsedRecordModelMapper.Create(profileKey, parsed);

                            row.RawContent = rawLine;
                            row.LineType = parsed.RecordType;
                            row.ParsedContent =
                                JsonSerializer.Serialize(parsed.ParsedDataModel, _parsedContentJsonOptions);
                            row.Status = FileRowStatus.Success;
                            row.RetryCount += 1;
                            row.Message = _localizer.Get("FileIngestion.ReprocessedSuccessfully");
                            ApplyCorrelation(row, profileKey, GetFileTypeFromProfileKey(profileKey),
                                parsed.ParsedDataModel);
                            _detailEntityMapper.AttachTypedDetail(row, profileKey, parsed.ParsedDataModel);
                        }
                        catch (Exception ex)
                        {
                            var exceptionDetail = ExceptionDetailHelper.BuildDetailMessage(ex, 2000);
                            if (errors.Count < MaxRetryErrorsPerAttempt)
                            {
                                var error = _ingestionErrorMapper.MapException(ex, "ROW_RETRY",
                                    fileName: sourceFile.Name);
                                error.LineNumber = row.LineNumber;
                                error.Detail = _localizer.Get("FileIngestion.Detail.RowRetryFailed", row.LineNumber,
                                    exceptionDetail);
                                errors.Add(error);
                            }

                            row.Status = FileRowStatus.Failed;
                            row.Message = exceptionDetail;
                            row.RetryCount += 1;
                            if (row.ReconciliationStatus != ReconciliationStatus.Success)
                                row.ReconciliationStatus = ReconciliationStatus.Failed;
                        }
                    }

                    _auditStampService.StampForUpdate(failedRows.Cast<AuditEntity>());
                    StampTypedDetails(failedRows);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    _dbContext.ChangeTracker.Clear();
                }
                
                const int maxExhaustedToReport = 10_000;
                var exhaustedRows = await _dbContext.IngestionFileLines
                    .AsNoTracking()
                    .Where(x => x.FileId == transactionFileId && x.Status == FileRowStatus.Failed)
                    .Where(x => x.RetryCount >= GetFailedRowMaxRetryCount())
                    .OrderBy(x => x.LineNumber)
                    .Take(maxExhaustedToReport + 1)
                    .Select(x => new { x.LineNumber, x.Message })
                    .ToListAsync(cancellationToken);

                var truncated = exhaustedRows.Count > maxExhaustedToReport;
                var reportLimit = truncated ? maxExhaustedToReport : exhaustedRows.Count;

                for (var er = 0; er < reportLimit; er++)
                {
                    var exhaustedRow = exhaustedRows[er];
                    errors.Add(new IngestionErrorDetail
                    {
                        Code = "ROW_RETRY_LIMIT",
                        Message = _localizer.Get("FileIngestion.RowRetryLimitExceeded", exhaustedRow.LineNumber),
                        Detail = exhaustedRow.Message,
                        Step = "ROW_RETRY",
                        LineNumber = exhaustedRow.LineNumber,
                        FileName = sourceFile.Name,
                        Severity = "Error"
                    });
                }

                if (truncated)
                {
                    errors.Add(new IngestionErrorDetail
                    {
                        Code = "ROW_RETRY_LIMIT_TRUNCATED",
                        Message = _localizer.Get("FileIngestion.RowRetryLimitExceeded", $">{maxExhaustedToReport}"),
                        Detail = _localizer.Get("FileIngestion.RowRetryLimitTruncatedDetail", maxExhaustedToReport),
                        Step = "ROW_RETRY",
                        FileName = sourceFile.Name,
                        Severity = "Warning"
                    });
                }

                await FinalizeFileStateAsync(transactionFileId, null, cancellationToken);
            }
            finally
            {
                if (sharedStream is not null)
                {
                    try
                    {
                        await sharedStream.DisposeAsync();
                    }
                    catch
                    {
                        /* ignore */
                    }
                }
            }
        }
        catch (Exception ex)
        {
            var error = _ingestionErrorMapper.MapException(ex, "RECOVERY_FAILED_ROWS", fileName: sourceFile.Name);
            error.Detail = _localizer.Get("FileIngestion.Detail.FailedRowBatchRetryFailed");
            errors.Add(error);
            throw;
        }
    }

    private static async Task<string> ReadRangeFromStreamAsync(
        Stream stream, long byteOffset, int byteLength, Encoding encoding, CancellationToken cancellationToken)
    {
        if (stream.Position != byteOffset)
            stream.Seek(byteOffset, SeekOrigin.Begin);

        var buffer = ArrayPool<byte>.Shared.Rent(byteLength);
        try
        {
            var totalRead = 0;
            while (totalRead < byteLength)
            {
                var read = await stream.ReadAsync(buffer.AsMemory(totalRead, byteLength - totalRead),
                    cancellationToken);
                if (read == 0) break;
                totalRead += read;
            }

            return encoding.GetString(buffer, 0, totalRead);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private async Task<IngestionFileEntity> RetryArchiveOnlyAsync(
        Guid transactionFileId,
        FileReference sourceFile,
        IFileTransferClient sourceTransferClient,
        List<IngestionErrorDetail> errors,
        CancellationToken cancellationToken)
    {
        IngestionFileEntity file;
        try
        {
            _dbContext.ChangeTracker.Clear();
            file = await GetFileAsync(transactionFileId, cancellationToken);
        }
        catch (Exception ex)
        {
            var error = _ingestionErrorMapper.MapDatabaseError(ex, fileName: sourceFile.Name);
            error.Step = "ARCHIVE_RETRY_INIT";
            error.Detail = _localizer.Get("FileIngestion.Detail.ArchiveRetryInitFailed");
            errors.Add(error);
            throw;
        }

        string? archiveErrorMessage = null;
        var auditStamp = _auditStampService.CreateStamp();
        const int maxArchiveAttempts = 3;

        for (var archiveAttempt = 1; archiveAttempt <= maxArchiveAttempts; archiveAttempt++)
        {
            var errorCountBeforeAttempt = errors.Count;
            archiveErrorMessage = null;

            try
            {
                var targetTransferClient = _fileTransferClientResolver.Create(
                    ResolveTargetSourceType(),
                    FileTransferEndpointType.Target);

                var profileKey = BuildProfileKey(file.FileType, file.ContentType);

                Stream targetStream;
                try
                {
                    targetStream = await targetTransferClient.OpenWriteAsync(
                        profileKey,
                        sourceFile.Name,
                        FileTransferEndpointType.Target,
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    var error = _ingestionErrorMapper.MapIOError(ex, fileName: sourceFile.Name);
                    error.Step = "ARCHIVE_STREAM_OPEN_TARGET";
                    error.Detail = _localizer.Get("FileIngestion.Detail.ArchiveStreamOpenTargetFailed");
                    errors.Add(error);
                    throw;
                }

                Stream sourceStream;
                try
                {
                    sourceStream = await sourceTransferClient.OpenReadAsync(sourceFile, cancellationToken);
                }
                catch (Exception ex)
                {
                    var error = _ingestionErrorMapper.MapIOError(ex, fileName: sourceFile.Name);
                    error.Step = "ARCHIVE_STREAM_OPEN_SOURCE";
                    error.Detail = _localizer.Get("FileIngestion.Detail.ArchiveStreamOpenSourceFailed");
                    errors.Add(error);
                    throw;
                }

                try
                {
                    await sourceStream.CopyToAsync(targetStream, cancellationToken);
                    await targetStream.FlushAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    var error = _ingestionErrorMapper.MapIOError(ex, fileName: sourceFile.Name);
                    error.Step = "ARCHIVE_STREAM_COPY";
                    error.Detail = _localizer.Get("FileIngestion.Detail.ArchiveStreamCopyFailed");
                    errors.Add(error);
                    throw;
                }
                finally
                {
                    await targetStream.DisposeAsync();
                    await sourceStream.DisposeAsync();
                }

                try
                {
                    var affectedRows = await _dbContext.IngestionFiles
                        .Where(x => x.Id == transactionFileId)
                        .ExecuteUpdateAsync(
                            setters => setters
                                .SetProperty(x => x.IsArchived, true)
                                .SetProperty(x => x.UpdateDate, auditStamp.Timestamp)
                                .SetProperty(x => x.LastModifiedBy, auditStamp.UserId),
                            cancellationToken);

                    if (affectedRows != 1)
                    {
                        throw new FileIngestionArchiveStatusUpdateRowMismatchException(
                            _localizer.Get("FileIngestion.ArchiveStatusUpdateRowMismatch", affectedRows,
                                transactionFileId));
                    }

                    _dbContext.ChangeTracker.Clear();
                }
                catch (Exception ex)
                {
                    var error = _ingestionErrorMapper.MapDatabaseError(ex, fileName: sourceFile.Name);
                    error.Step = "ARCHIVE_MARK_COMPLETE";
                    error.Detail = _localizer.Get("FileIngestion.Detail.ArchiveMarkCompleteFailed");
                    errors.Add(error);
                    throw;
                }

                break;
            }
            catch (Exception ex)
            {
                archiveErrorMessage = $"[{ex.GetType().Name}] {ex.Message}";
                if (ex.InnerException != null)
                    archiveErrorMessage += $" -> [{ex.InnerException.GetType().Name}] {ex.InnerException.Message}";

                if (archiveAttempt < maxArchiveAttempts)
                {
                    errors.RemoveRange(errorCountBeforeAttempt, errors.Count - errorCountBeforeAttempt);
                    _dbContext.ChangeTracker.Clear();
                    await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
                    continue;
                }

                try
                {
                    await _dbContext.IngestionFiles
                        .Where(x => x.Id == transactionFileId)
                        .ExecuteUpdateAsync(
                            setters => setters
                                .SetProperty(x => x.IsArchived, false)
                                .SetProperty(x => x.UpdateDate, auditStamp.Timestamp)
                                .SetProperty(x => x.LastModifiedBy, auditStamp.UserId),
                            cancellationToken);

                    _dbContext.ChangeTracker.Clear();
                }
                catch (Exception exMarkIncomplete)
                {
                    var error = _ingestionErrorMapper.MapDatabaseError(exMarkIncomplete, fileName: sourceFile.Name);
                    error.Step = "ARCHIVE_MARK_INCOMPLETE";
                    error.Detail = _localizer.Get("FileIngestion.Detail.ArchiveMarkIncompleteFailed");
                    errors.Add(error);
                }
            }
        }

        try
        {
            await FinalizeFileStateAsync(transactionFileId, archiveErrorMessage, cancellationToken);
        }
        catch (Exception ex)
        {
            var error = _ingestionErrorMapper.MapDatabaseError(ex, fileName: sourceFile.Name);
            error.Step = "ARCHIVE_FINALIZE";
            error.Detail = _localizer.Get("FileIngestion.Detail.ArchiveRetryFinalizeFailed");
            errors.Add(error);
        }

        try
        {
            _dbContext.ChangeTracker.Clear();
            return await GetFileAsync(transactionFileId, cancellationToken);
        }
        catch (Exception ex)
        {
            var error = _ingestionErrorMapper.MapDatabaseError(ex, fileName: sourceFile.Name);
            error.Step = "ARCHIVE_FINAL_RETRIEVAL";
            error.Detail = _localizer.Get("FileIngestion.Detail.ArchiveRetryFinalRetrievalFailed");
            errors.Add(error);
            throw;
        }
    }

    private async Task<bool> RequiresReArchiveAsync(
        Guid transactionFileId,
        CancellationToken cancellationToken)
    {
        var file = await _dbContext.IngestionFiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == transactionFileId, cancellationToken);

        if (file is null || !file.IsArchived)
            return false;

        var detailMetrics = await _dbContext.IngestionFileLines
            .AsNoTracking()
            .Where(x => x.FileId == transactionFileId && x.LineType == "D")
            .GroupBy(x => 1)
            .Select(group => new
            {
                LastCreateDate = group.Max(x => (DateTime?)x.CreateDate),
                LastUpdateDate = group.Max(x => x.UpdateDate)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (detailMetrics is null)
            return false;

        var lastDetailChangeDate = detailMetrics.LastUpdateDate ?? detailMetrics.LastCreateDate;
        if (lastDetailChangeDate is null)
            return false;

        var lastFileFinalizeDate = file.UpdateDate ?? file.CreateDate;

        return lastDetailChangeDate > lastFileFinalizeDate;
    }

    private async Task PersistBatchAsync(
        Guid transactionFileId,
        List<IngestionFileLineEntity> batch,
        ProcessingProgress progress,
        CancellationToken cancellationToken,
        bool forceCheckpoint = false)
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            await PersistIngestionFileLineBatchAsync(batch, cancellationToken);
            
            progress.BatchSequence++;
            var checkpointEvery = _options.Processing.CheckpointEveryNBatches ??
                                  ProcessingOptions.DefaultCheckpointEveryNBatches;
            if (forceCheckpoint || (progress.BatchSequence % checkpointEvery) == 0)
            {
                var auditStamp = _auditStampService.CreateStamp();
                await _dbContext.IngestionFiles
                    .Where(x => x.Id == transactionFileId)
                    .ExecuteUpdateAsync(
                        setters => setters
                            .SetProperty(x => x.LastProcessedLineNumber, progress.LastProcessedLineNumber)
                            .SetProperty(x => x.LastProcessedByteOffset, progress.LastProcessedByteOffset)
                            .SetProperty(x => x.UpdateDate, auditStamp.Timestamp)
                            .SetProperty(x => x.LastModifiedBy, auditStamp.UserId),
                        cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
        });

        _dbContext.ChangeTracker.Clear();
        batch.Clear();
    }

    private async Task PersistIngestionFileLineBatchAsync(
        IReadOnlyList<IngestionFileLineEntity> rows,
        CancellationToken cancellationToken)
    {
        if (rows.Count == 0)
            return;

        if (_options.Processing.UseBulkInsert != true)
        {
            _auditStampService.StampForCreate(rows.Cast<AuditEntity>());
            StampTypedDetails(rows);
            await _dbContext.IngestionFileLines.AddRangeAsync(rows, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        var providerName = _dbContext.Database.ProviderName ?? string.Empty;

        if (providerName.Contains("SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            await BulkInsertSqlServerAsync(rows, cancellationToken);
            await PersistTypedDetailEntitiesAsync(rows, cancellationToken);
            return;
        }

        if (providerName.Contains("Npgsql", StringComparison.OrdinalIgnoreCase))
        {
            await BulkInsertPostgreSqlAsync(rows, cancellationToken);
            await PersistTypedDetailEntitiesAsync(rows, cancellationToken);
            return;
        }

        _auditStampService.StampForCreate(rows.Cast<AuditEntity>());
        StampTypedDetails(rows);
        await _dbContext.IngestionFileLines.AddRangeAsync(rows, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task PersistTypedDetailEntitiesAsync(
        IReadOnlyList<IngestionFileLineEntity> rows,
        CancellationToken cancellationToken)
    {
        var detailsByType = new Dictionary<Type, List<AuditEntity>>();
        foreach (var row in rows)
        {
            var detail = ExtractTypedDetail(row);
            if (detail is null) continue;
            ((IIngestionTypedDetail)detail).FileLineId = row.Id;

            var t = detail.GetType();
            if (!detailsByType.TryGetValue(t, out var list))
            {
                list = new List<AuditEntity>();
                detailsByType[t] = list;
            }

            list.Add(detail);
        }

        if (detailsByType.Count == 0)
            return;
        
        var providerName = _dbContext.Database.ProviderName ?? string.Empty;
        var isPostgres = providerName.Contains("Npgsql", StringComparison.OrdinalIgnoreCase);
        var isSqlServer = providerName.Contains("SqlServer", StringComparison.OrdinalIgnoreCase);
        var bulkEnabled = _options.Processing.UseBulkInsert == true;

        foreach (var kvp in detailsByType)
        {
            var typedDetails = kvp.Value;
            _auditStampService.StampForCreate(typedDetails);

            if (!bulkEnabled || (!isPostgres && !isSqlServer))
            {
                const int fallbackChunk = 1000;
                for (var off = 0; off < typedDetails.Count; off += fallbackChunk)
                {
                    var len = Math.Min(fallbackChunk, typedDetails.Count - off);
                    _dbContext.AddRange(typedDetails.GetRange(off, len));
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    _dbContext.ChangeTracker.Clear();
                }

                continue;
            }

            if (isSqlServer)
                await BulkInsertGenericSqlServerAsync(kvp.Key, typedDetails, cancellationToken);
            else
                await BulkInsertGenericPostgresAsync(kvp.Key, typedDetails, cancellationToken);
        }
    }
    
    private static readonly ConcurrentDictionary<Type, IReadOnlyList<IProperty>> _genericBulkPropertyCache = new();

    private static IReadOnlyList<IProperty> GetGenericBulkProperties(IEntityType entityType,
        StoreObjectIdentifier storeObject)
    {
        return _genericBulkPropertyCache.GetOrAdd(entityType.ClrType, _ => entityType.GetProperties()
            .Where(p => !p.IsShadowProperty() && p.GetColumnName(storeObject) is not null)
            .ToArray());
    }

    private async Task BulkInsertGenericSqlServerAsync(
        Type entityClrType,
        IReadOnlyList<AuditEntity> rows,
        CancellationToken cancellationToken)
    {
        var entityType = _dbContext.Model.FindEntityType(entityClrType)
                         ?? throw new FileIngestionEntityTypeNotMappedException(
                             _localizer.Get("FileIngestion.EntityTypeNotMapped"));
        var storeObject = StoreObjectIdentifier.Table(entityType.GetTableName()!, entityType.GetSchema());
        var properties = GetGenericBulkProperties(entityType, storeObject);

        var connection = (SqlConnection)_dbContext.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);
        var transaction = (SqlTransaction?)_dbContext.Database.CurrentTransaction?.GetDbTransaction();

        var destinationTableName = string.IsNullOrWhiteSpace(entityType.GetSchema())
            ? $"[{entityType.GetTableName()}]"
            : $"[{entityType.GetSchema()}].[{entityType.GetTableName()}]";
        
        using var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction)
        {
            DestinationTableName = destinationTableName,
            BatchSize = Math.Min(rows.Count, BulkCopyInternalBatchSize),
            BulkCopyTimeout = BulkCopyTimeoutSeconds,
            EnableStreaming = true
        };
        for (var i = 0; i < properties.Count; i++)
            bulkCopy.ColumnMappings.Add(i, properties[i].GetColumnName(storeObject)!);

        using var reader = new GenericPropertyDataReader(rows, properties);
        await bulkCopy.WriteToServerAsync(reader, cancellationToken);
    }

    private async Task BulkInsertGenericPostgresAsync(
        Type entityClrType,
        IReadOnlyList<AuditEntity> rows,
        CancellationToken cancellationToken)
    {
        var entityType = _dbContext.Model.FindEntityType(entityClrType)
                         ?? throw new FileIngestionEntityTypeNotMappedException(
                             _localizer.Get("FileIngestion.EntityTypeNotMapped"));
        var storeObject = StoreObjectIdentifier.Table(entityType.GetTableName()!, entityType.GetSchema());
        var properties = GetGenericBulkProperties(entityType, storeObject);

        var connection = (NpgsqlConnection)_dbContext.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        var schema = string.IsNullOrWhiteSpace(entityType.GetSchema())
            ? string.Empty
            : $"\"{entityType.GetSchema()}\".";
        var table = $"\"{entityType.GetTableName()}\"";
        var columnNames = string.Join(", ", properties.Select(p => $"\"{p.GetColumnName(storeObject)}\""));
        var copyCommand = $"COPY {schema}{table} ({columnNames}) FROM STDIN (FORMAT BINARY)";

        await using var importer = await connection.BeginBinaryImportAsync(copyCommand, cancellationToken);
        foreach (var row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            importer.StartRow();
            foreach (var property in properties)
            {
                var value = GetDatabaseValue(property, row);
                if (value is null)
                {
                    importer.WriteNull();
                    continue;
                }

                WriteNpgsqlValue(importer, value, property.ClrType);
            }
        }

        await importer.CompleteAsync(cancellationToken);
    }
    
    private sealed class GenericPropertyDataReader : IDataReader
    {
        private readonly IReadOnlyList<AuditEntity> _rows;
        private readonly IReadOnlyList<IProperty> _properties;
        private readonly Func<object, object?>[] _getters;
        private readonly bool[] _isEnum;
        private readonly Type[] _fieldTypes;
        private readonly object?[] _currentValues;
        private int _index = -1;

        public GenericPropertyDataReader(IReadOnlyList<AuditEntity> rows, IReadOnlyList<IProperty> properties)
        {
            _rows = rows;
            _properties = properties;
            var count = properties.Count;
            _getters = new Func<object, object?>[count];
            _isEnum = new bool[count];
            _fieldTypes = new Type[count];
            _currentValues = new object?[count];
            
            var entityClr = rows.Count > 0 ? rows[0].GetType() : typeof(AuditEntity);
            for (var i = 0; i < count; i++)
            {
                var p = properties[i];
                var clr = Nullable.GetUnderlyingType(p.ClrType) ?? p.ClrType;
                _isEnum[i] = clr.IsEnum;
                _fieldTypes[i] = clr.IsEnum ? typeof(string) : clr;
                _getters[i] = BuildGetter(entityClr, p.PropertyInfo!.Name);
            }
        }

        public int FieldCount => _properties.Count;

        public bool Read()
        {
            _index++;
            if (_index >= _rows.Count) return false;
            var row = _rows[_index];
            for (var i = 0; i < _getters.Length; i++)
            {
                var raw = _getters[i](row);
                if (raw is null)
                {
                    _currentValues[i] = null;
                    continue;
                }

                if (_isEnum[i])
                {
                    var serializer = _enumToStringCache.GetOrAdd(raw.GetType(), static t =>
                    {
                        var p = Expression.Parameter(typeof(object), "v");
                        var cast = Expression.Convert(p, t);
                        var toStr = Expression.Call(cast, t.GetMethod(nameof(object.ToString), Type.EmptyTypes)!);
                        return Expression.Lambda<Func<object, string>>(toStr, p).Compile();
                    });
                    _currentValues[i] = serializer(raw);
                }
                else
                {
                    _currentValues[i] = raw;
                }
            }

            return true;
        }

        public object GetValue(int i) => _currentValues[i] ?? DBNull.Value;

        public int GetValues(object[] values)
        {
            var n = Math.Min(values.Length, _properties.Count);
            for (var i = 0; i < n; i++) values[i] = _currentValues[i] ?? DBNull.Value;
            return n;
        }

        public bool IsDBNull(int i) => _currentValues[i] is null;
        public string GetName(int i) => _properties[i].Name;

        public int GetOrdinal(string name)
        {
            for (var i = 0; i < _properties.Count; i++)
                if (string.Equals(_properties[i].Name, name, StringComparison.Ordinal))
                    return i;
            return -1;
        }

        public Type GetFieldType(int i) => _fieldTypes[i];

        public void Close()
        {
        }

        public void Dispose()
        {
        }

        public int Depth => 0;
        public bool IsClosed => false;
        public int RecordsAffected => -1;
        public bool NextResult() => false;
        public DataTable? GetSchemaTable() => null;
        public string GetDataTypeName(int i) => GetFieldType(i).Name;
        public bool GetBoolean(int i) => (bool)GetValue(i);
        public byte GetByte(int i) => (byte)GetValue(i);

        public long GetBytes(int i, long fieldOffset, byte[]? buffer, int bufferoffset, int length) =>
            throw new NotSupportedException();

        public char GetChar(int i) => (char)GetValue(i);

        public long GetChars(int i, long fieldoffset, char[]? buffer, int bufferoffset, int length) =>
            throw new NotSupportedException();

        public IDataReader GetData(int i) => throw new NotSupportedException();
        public DateTime GetDateTime(int i) => (DateTime)GetValue(i);
        public decimal GetDecimal(int i) => (decimal)GetValue(i);
        public double GetDouble(int i) => (double)GetValue(i);
        public float GetFloat(int i) => (float)GetValue(i);
        public Guid GetGuid(int i) => (Guid)GetValue(i);
        public short GetInt16(int i) => (short)GetValue(i);
        public int GetInt32(int i) => (int)GetValue(i);
        public long GetInt64(int i) => (long)GetValue(i);
        public string GetString(int i) => (string)GetValue(i);
        public object this[int i] => GetValue(i);
        public object this[string name] => GetValue(GetOrdinal(name));
    }

    private void StampTypedDetails(IReadOnlyList<IngestionFileLineEntity> rows)
    {
        List<AuditEntity>? details = null;
        for (var i = 0; i < rows.Count; i++)
        {
            var d = ExtractTypedDetail(rows[i]);
            if (d is null) continue;
            details ??= new List<AuditEntity>(rows.Count);
            details.Add(d);
        }

        if (details is { Count: > 0 })
            _auditStampService.StampForCreate(details);
    }

    private static AuditEntity? ExtractTypedDetail(IngestionFileLineEntity row)
    {
        if (row.CardVisaDetail is not null) return row.CardVisaDetail;
        if (row.CardMscDetail is not null) return row.CardMscDetail;
        if (row.CardBkmDetail is not null) return row.CardBkmDetail;
        if (row.ClearingVisaDetail is not null) return row.ClearingVisaDetail;
        if (row.ClearingMscDetail is not null) return row.ClearingMscDetail;
        if (row.ClearingBkmDetail is not null) return row.ClearingBkmDetail;
        return null;
    }

    private async Task BulkInsertSqlServerAsync(
        IReadOnlyList<IngestionFileLineEntity> rows,
        CancellationToken cancellationToken)
    {
        try
        {
            var entityType = _dbContext.Model.FindEntityType(typeof(IngestionFileLineEntity))
                             ?? throw new FileIngestionEntityTypeNotMappedException(
                                 _localizer.Get("FileIngestion.EntityTypeNotMapped"));

            var storeObject = StoreObjectIdentifier.Table(entityType.GetTableName()!, entityType.GetSchema());
            var properties = GetBulkProperties(entityType, storeObject);

            _auditStampService.StampForCreate(rows.Cast<AuditEntity>());

            var connection = (SqlConnection)_dbContext.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync(cancellationToken);

            var transaction = (SqlTransaction?)_dbContext.Database.CurrentTransaction?.GetDbTransaction();

            var destinationTableName = string.IsNullOrWhiteSpace(entityType.GetSchema())
                ? $"[{entityType.GetTableName()}]"
                : $"[{entityType.GetSchema()}].[{entityType.GetTableName()}]";

            using var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction)
            {
                DestinationTableName = destinationTableName,
                BatchSize = Math.Min(rows.Count, BulkCopyInternalBatchSize),
                BulkCopyTimeout = BulkCopyTimeoutSeconds,
                EnableStreaming = true
            };

            for (var ordinal = 0; ordinal < properties.Count; ordinal++)
            {
                var columnName = properties[ordinal].GetColumnName(storeObject)!;
                bulkCopy.ColumnMappings.Add(ordinal, columnName);
            }
            
            using var reader = new BulkPropertyDataReader(rows, properties);
            await bulkCopy.WriteToServerAsync(reader, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new FileIngestionSqlBulkInsertFailedException(
                _localizer.Get("FileIngestion.SqlBulkInsertFailed", ex.Message),
                ex);
        }
    }
    
    private sealed class BulkPropertyDataReader : IDataReader
    {
        private readonly IReadOnlyList<IngestionFileLineEntity> _rows;
        private readonly IReadOnlyList<IProperty> _properties;
        private readonly Func<object, object?>[] _getters;
        private readonly bool[] _isEnum;
        private readonly string[] _names;
        private readonly Type[] _fieldTypes;
        private readonly object?[] _currentValues;
        private int _index = -1;

        public BulkPropertyDataReader(
            IReadOnlyList<IngestionFileLineEntity> rows,
            IReadOnlyList<IProperty> properties)
        {
            _rows = rows;
            _properties = properties;

            var count = properties.Count;
            _getters = new Func<object, object?>[count];
            _isEnum = new bool[count];
            _names = new string[count];
            _fieldTypes = new Type[count];
            _currentValues = new object?[count];

            for (var i = 0; i < count; i++)
            {
                var p = properties[i];
                _names[i] = p.Name;
                var clr = Nullable.GetUnderlyingType(p.ClrType) ?? p.ClrType;
                _isEnum[i] = clr.IsEnum;
                _fieldTypes[i] = clr.IsEnum ? typeof(string) : clr;
                _getters[i] = BuildGetter(typeof(IngestionFileLineEntity), p.PropertyInfo!.Name);
            }
        }

        public int FieldCount => _properties.Count;

        public bool Read()
        {
            _index++;
            if (_index >= _rows.Count) return false;
            var row = _rows[_index];
            for (var i = 0; i < _getters.Length; i++)
            {
                var raw = _getters[i](row);
                if (raw is null)
                {
                    _currentValues[i] = null;
                    continue;
                }

                if (_isEnum[i])
                {
                    var serializer = _enumToStringCache.GetOrAdd(raw.GetType(), static t =>
                    {
                        var p = Expression.Parameter(typeof(object), "v");
                        var cast = Expression.Convert(p, t);
                        var toStr = Expression.Call(cast, t.GetMethod(nameof(object.ToString), Type.EmptyTypes)!);
                        return Expression.Lambda<Func<object, string>>(toStr, p).Compile();
                    });
                    _currentValues[i] = serializer(raw);
                }
                else
                {
                    _currentValues[i] = raw;
                }
            }

            return true;
        }

        public object GetValue(int i) => _currentValues[i] ?? DBNull.Value;

        public int GetValues(object[] values)
        {
            var count = Math.Min(values.Length, _properties.Count);
            for (var i = 0; i < count; i++)
                values[i] = _currentValues[i] ?? DBNull.Value;
            return count;
        }

        public bool IsDBNull(int i) => _currentValues[i] is null;
        public string GetName(int i) => _names[i];

        public int GetOrdinal(string name)
        {
            for (var i = 0; i < _names.Length; i++)
                if (string.Equals(_names[i], name, StringComparison.Ordinal))
                    return i;
            return -1;
        }

        public Type GetFieldType(int i) => _fieldTypes[i];

        public void Close()
        {
        }

        public void Dispose()
        {
        }

        public int Depth => 0;
        public bool IsClosed => false;
        public int RecordsAffected => -1;
        public bool NextResult() => false;
        public DataTable? GetSchemaTable() => null;
        public string GetDataTypeName(int i) => GetFieldType(i).Name;
        public bool GetBoolean(int i) => (bool)GetValue(i);
        public byte GetByte(int i) => (byte)GetValue(i);

        public long GetBytes(int i, long fieldOffset, byte[]? buffer, int bufferoffset, int length) =>
            throw new NotSupportedException();

        public char GetChar(int i) => (char)GetValue(i);

        public long GetChars(int i, long fieldoffset, char[]? buffer, int bufferoffset, int length) =>
            throw new NotSupportedException();

        public IDataReader GetData(int i) => throw new NotSupportedException();
        public DateTime GetDateTime(int i) => (DateTime)GetValue(i);
        public decimal GetDecimal(int i) => (decimal)GetValue(i);
        public double GetDouble(int i) => (double)GetValue(i);
        public float GetFloat(int i) => (float)GetValue(i);
        public Guid GetGuid(int i) => (Guid)GetValue(i);
        public short GetInt16(int i) => (short)GetValue(i);
        public int GetInt32(int i) => (int)GetValue(i);
        public long GetInt64(int i) => (long)GetValue(i);
        public string GetString(int i) => (string)GetValue(i);
        public object this[int i] => GetValue(i);
        public object this[string name] => GetValue(GetOrdinal(name));
    }

    private static Func<object, object?> BuildGetter(Type entityType, string propertyName)
    {
        return _propertyGetterCache.GetOrAdd((entityType, propertyName), static k =>
        {
            var paramExpr = Expression.Parameter(typeof(object), "instance");
            var castExpr = Expression.Convert(paramExpr, k.Item1);
            var propExpr = Expression.Property(castExpr, k.Item2);
            var boxExpr = Expression.Convert(propExpr, typeof(object));
            return Expression.Lambda<Func<object, object?>>(boxExpr, paramExpr).Compile();
        });
    }

    private async Task BulkInsertPostgreSqlAsync(
        IReadOnlyList<IngestionFileLineEntity> rows,
        CancellationToken cancellationToken)
    {
        try
        {
            var entityType = _dbContext.Model.FindEntityType(typeof(IngestionFileLineEntity))
                             ?? throw new FileIngestionEntityTypeNotMappedException(
                                 _localizer.Get("FileIngestion.EntityTypeNotMapped"));

            var storeObject = StoreObjectIdentifier.Table(entityType.GetTableName()!, entityType.GetSchema());
            var properties = GetBulkProperties(entityType, storeObject);

            var columnNames = properties
                .Select(property => $"\"{property.GetColumnName(storeObject)}\"")
                .ToArray();

            var connection = (NpgsqlConnection)_dbContext.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync(cancellationToken);

            var efTransaction = _dbContext.Database.CurrentTransaction
                                ?? throw new FileIngestionPostgreBulkInsertFailedException(
                                    _localizer.Get("FileIngestion.PostgreBulkInsertFailed",
                                        _localizer.Get("FileIngestion.Detail.NoActiveEfTransactionForCopy")));
            var npgsqlTransaction = (NpgsqlTransaction)efTransaction.GetDbTransaction();

            var schema = string.IsNullOrWhiteSpace(entityType.GetSchema())
                ? string.Empty
                : $"\"{entityType.GetSchema()}\".";
            var table = $"\"{entityType.GetTableName()}\"";
            var copyCommand = $"COPY {schema}{table} ({string.Join(", ", columnNames)}) FROM STDIN (FORMAT BINARY)";

            await using var importer = await connection.BeginBinaryImportAsync(copyCommand, cancellationToken);

            _auditStampService.StampForCreate(rows.Cast<AuditEntity>());
            foreach (var row in rows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                importer.StartRow();

                foreach (var property in properties)
                {
                    var value = GetDatabaseValue(property, row);
                    if (value is null)
                    {
                        importer.WriteNull();
                        continue;
                    }

                    WriteNpgsqlValue(importer, value, property.ClrType);
                }
            }

            await importer.CompleteAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new FileIngestionPostgreBulkInsertFailedException(
                _localizer.Get("FileIngestion.PostgreBulkInsertFailed", ex.Message),
                ex);
        }
    }


    private IReadOnlyList<IProperty> GetBulkProperties(
        IEntityType entityType,
        StoreObjectIdentifier storeObject)
    {
        if (_ingestionBulkPropertyCache.TryGetValue(entityType.ClrType, out var cached))
            return cached;

        string[] propertyNames;

        if (entityType.ClrType == typeof(IngestionFileLineEntity))
        {
            propertyNames =
            [
                nameof(IngestionFileLineEntity.Id),
                nameof(IngestionFileLineEntity.FileId),
                nameof(IngestionFileLineEntity.LineNumber),
                nameof(IngestionFileLineEntity.ByteOffset),
                nameof(IngestionFileLineEntity.ByteLength),
                nameof(IngestionFileLineEntity.LineType),
                nameof(IngestionFileLineEntity.RawContent),
                nameof(IngestionFileLineEntity.ParsedContent),
                nameof(IngestionFileLineEntity.Status),
                nameof(IngestionFileLineEntity.Message),
                nameof(IngestionFileLineEntity.RetryCount),
                nameof(IngestionFileLineEntity.CorrelationKey),
                nameof(IngestionFileLineEntity.CorrelationValue),
                nameof(IngestionFileLineEntity.DuplicateDetectionKey),
                nameof(IngestionFileLineEntity.DuplicateStatus),
                nameof(IngestionFileLineEntity.DuplicateGroupId),
                nameof(IngestionFileLineEntity.ReconciliationStatus),
                nameof(IngestionFileLineEntity.MatchedClearingLineId),
                nameof(IngestionFileLineEntity.CreateDate),
                nameof(IngestionFileLineEntity.CreatedBy),
                nameof(IngestionFileLineEntity.UpdateDate),
                nameof(IngestionFileLineEntity.LastModifiedBy),
                nameof(IngestionFileLineEntity.RecordStatus)
            ];
        }
        else if (entityType.ClrType == typeof(IngestionFileEntity))
        {
            propertyNames =
            [
                nameof(IngestionFileEntity.Id),
                nameof(IngestionFileEntity.FileKey),
                nameof(IngestionFileEntity.FileName),
                nameof(IngestionFileEntity.FilePath),
                nameof(IngestionFileEntity.SourceType),
                nameof(IngestionFileEntity.FileType),
                nameof(IngestionFileEntity.ContentType),
                nameof(IngestionFileEntity.Status),
                nameof(IngestionFileEntity.Message),
                nameof(IngestionFileEntity.ExpectedLineCount),
                nameof(IngestionFileEntity.ProcessedLineCount),
                nameof(IngestionFileEntity.SuccessfulLineCount),
                nameof(IngestionFileEntity.FailedLineCount),
                nameof(IngestionFileEntity.LastProcessedLineNumber),
                nameof(IngestionFileEntity.LastProcessedByteOffset),
                nameof(IngestionFileEntity.IsArchived),
                nameof(IngestionFileEntity.CreateDate),
                nameof(IngestionFileEntity.CreatedBy),
                nameof(IngestionFileEntity.UpdateDate),
                nameof(IngestionFileEntity.LastModifiedBy),
                nameof(IngestionFileEntity.RecordStatus)
            ];
        }
        else
        {
            throw new FileIngestionBulkPropertyMappingNotDefinedException(
                _localizer.Get("FileIngestion.BulkPropertyMappingNotDefined", entityType.ClrType.Name));
        }

        var resolved = propertyNames
            .Select(name => entityType.FindProperty(name)
                            ?? throw new FileIngestionPropertyNotMappedException(
                                _localizer.Get("FileIngestion.PropertyNotMapped", name, entityType.Name)))
            .Where(property => property.GetColumnName(storeObject) is not null)
            .ToArray();
        
        _ingestionBulkPropertyCache[entityType.ClrType] = resolved;
        return resolved;
    }

    private static object? GetDatabaseValue(IProperty property, object entity)
    {
        var propInfo = property.PropertyInfo;
        if (propInfo is null)
            return null;

        var key = (entity.GetType(), propInfo.Name);
        var getter = _propertyGetterCache.GetOrAdd(key, static k =>
        {
            var paramExpr = Expression.Parameter(typeof(object), "instance");
            var castExpr = Expression.Convert(paramExpr, k.Item1);
            var propExpr = Expression.Property(castExpr, k.Item2);
            var boxExpr = Expression.Convert(propExpr, typeof(object));
            return Expression.Lambda<Func<object, object?>>(boxExpr, paramExpr).Compile();
        });

        var value = getter(entity);
        if (value is null)
            return null;

        var clrType = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;
        if (!clrType.IsEnum)
            return value;
        
        var enumSerializer = _enumToStringCache.GetOrAdd(clrType, static t =>
        {
            var p = Expression.Parameter(typeof(object), "v");
            var cast = Expression.Convert(p, t);
            var toStr = Expression.Call(cast, t.GetMethod(nameof(object.ToString), Type.EmptyTypes)!);
            return Expression.Lambda<Func<object, string>>(toStr, p).Compile();
        });
        return enumSerializer(value);
    }

    private void WriteNpgsqlValue(
        NpgsqlBinaryImporter importer,
        object value,
        Type clrType)
    {
        var effectiveType = Nullable.GetUnderlyingType(clrType) ?? clrType;

        if (effectiveType.IsEnum)
        {
            importer.Write(value is string s ? s : value.ToString()!, NpgsqlDbType.Text);
            return;
        }

        if (effectiveType == typeof(Guid))
        {
            importer.Write((Guid)value, NpgsqlDbType.Uuid);
            return;
        }

        if (effectiveType == typeof(string))
        {
            importer.Write((string)value, NpgsqlDbType.Text);
            return;
        }

        if (effectiveType == typeof(long))
        {
            importer.Write((long)value, NpgsqlDbType.Bigint);
            return;
        }

        if (effectiveType == typeof(int))
        {
            importer.Write((int)value, NpgsqlDbType.Integer);
            return;
        }

        if (effectiveType == typeof(short))
        {
            importer.Write((short)value, NpgsqlDbType.Smallint);
            return;
        }

        if (effectiveType == typeof(bool))
        {
            importer.Write((bool)value, NpgsqlDbType.Boolean);
            return;
        }

        if (effectiveType == typeof(DateTime))
        {
            importer.Write((DateTime)value, NpgsqlDbType.Timestamp);
            return;
        }

        if (effectiveType == typeof(decimal))
        {
            importer.Write((decimal)value, NpgsqlDbType.Numeric);
            return;
        }

        if (effectiveType == typeof(double))
        {
            importer.Write((double)value, NpgsqlDbType.Double);
            return;
        }

        if (effectiveType == typeof(float))
        {
            importer.Write((float)value, NpgsqlDbType.Real);
            return;
        }

        if (effectiveType == typeof(byte))
        {
            importer.Write((short)(byte)value, NpgsqlDbType.Smallint);
            return;
        }

        if (effectiveType == typeof(sbyte))
        {
            importer.Write((short)(sbyte)value, NpgsqlDbType.Smallint);
            return;
        }

        if (effectiveType == typeof(char))
        {
            importer.Write(((char)value).ToString(), NpgsqlDbType.Text);
            return;
        }

        if (effectiveType == typeof(DateTimeOffset))
        {
            importer.Write((DateTimeOffset)value, NpgsqlDbType.TimestampTz);
            return;
        }

        if (effectiveType == typeof(DateOnly))
        {
            importer.Write(((DateOnly)value).ToDateTime(TimeOnly.MinValue), NpgsqlDbType.Date);
            return;
        }

        if (effectiveType == typeof(TimeOnly))
        {
            importer.Write(((TimeOnly)value).ToTimeSpan(), NpgsqlDbType.Time);
            return;
        }

        if (effectiveType == typeof(TimeSpan))
        {
            importer.Write((TimeSpan)value, NpgsqlDbType.Interval);
            return;
        }

        if (effectiveType == typeof(byte[]))
        {
            importer.Write((byte[])value, NpgsqlDbType.Bytea);
            return;
        }

        throw new FileIngestionUnsupportedBulkTypeException(
            _localizer.Get("FileIngestion.UnsupportedBulkType", effectiveType.Name));
    }

    private IngestionFileLineEntity BuildIngestionFileLine(
        Guid transactionFileId,
        string profileKey,
        FileType fileType,
        FileLineReadResult lineReadResult,
        ParsingOptions parsingRule)
    {
        var recordType = string.IsNullOrWhiteSpace(lineReadResult.Line)
            ? string.Empty
            : lineReadResult.Line[0] switch
            {
                'H' => "H",
                'F' => "F",
                _ => "D"
            };

        try
        {
            var parsed = _fixedWidthRecordParser.Parse(lineReadResult.Line, parsingRule);
            parsed.ParsedDataModel = _parsedRecordModelMapper.Create(profileKey, parsed);

            var entity = new IngestionFileLineEntity
            {
                FileId = transactionFileId,
                LineNumber = lineReadResult.LineNumber,
                ByteOffset = lineReadResult.ByteOffset,
                ByteLength = lineReadResult.ByteLength,
                LineType = parsed.RecordType,
                RawContent = lineReadResult.Line,
                ParsedContent = JsonSerializer.Serialize(parsed.ParsedDataModel, _parsedContentJsonOptions),
                Status = FileRowStatus.Success,
                Message = _localizer.Get("FileIngestion.ParsedAndPersistedSuccessfully"),
                RetryCount = 0
            };

            ApplyCorrelation(entity, profileKey, fileType, parsed.ParsedDataModel);
            _detailEntityMapper.AttachTypedDetail(entity, profileKey, parsed.ParsedDataModel);
            return entity;
        }
        catch (Exception ex)
        {
            return new IngestionFileLineEntity
            {
                FileId = transactionFileId,
                LineNumber = lineReadResult.LineNumber,
                ByteOffset = lineReadResult.ByteOffset,
                ByteLength = lineReadResult.ByteLength,
                LineType = recordType,
                RawContent = lineReadResult.Line,
                Status = FileRowStatus.Failed,
                Message = ExceptionDetailHelper.BuildDetailMessage(ex, 500),
                RetryCount = 0,
                ReconciliationStatus = recordType == "D" ? ReconciliationStatus.Failed : null
            };
        }
    }

    private void ApplyCorrelation(
        IngestionFileLineEntity row,
        string profileKey,
        FileType fileType,
        object parsedDataModel)
    {
        if (!string.Equals(row.LineType, "D", StringComparison.OrdinalIgnoreCase))
            return;

        if (TryGetLongValue(parsedDataModel, "OceanTxnGuid", out var oceanTxnGuid) && oceanTxnGuid > 0)
        {
            row.CorrelationKey = "OceanTxnGuid";
            row.CorrelationValue = oceanTxnGuid.ToString(CultureInfo.InvariantCulture);
            row.ReconciliationStatus = ReconciliationStatus.Ready;

            if (fileType == FileType.Card)
            {
                row.DuplicateDetectionKey = row.CorrelationValue;
            }
            else if (fileType == FileType.Clearing)
            {
                row.DuplicateDetectionKey = BuildClearingDuplicateDetectionKey(parsedDataModel);
            }

            return;
        }

        row.CorrelationKey = "Rrn:CardNo:ProvisionCode:Arn:Mcc:Amount:Currency";
        string v0, v1, v2, v3, v4, v5, v6;
        switch (fileType)
        {
            case FileType.Card:
                v0 = GetStringValue(parsedDataModel, "Rrn");
                v1 = GetStringValue(parsedDataModel, "CardNo");
                v2 = GetStringValue(parsedDataModel, "ProvisionCode");
                v3 = GetStringValue(parsedDataModel, "Arn");
                v4 = GetNormalizedValue(parsedDataModel, "Mcc");
                v5 = GetNormalizedValue(parsedDataModel, "CardHolderBillingAmount");
                v6 = GetNormalizedValue(parsedDataModel, "CardHolderBillingCurrency");
                break;
            case FileType.Clearing:
                v0 = GetStringValue(parsedDataModel, "Rrn");
                v1 = GetStringValue(parsedDataModel, "CardNo");
                v2 = GetStringValue(parsedDataModel, "ProvisionCode");
                v3 = GetStringValue(parsedDataModel, "Arn");
                v4 = GetNormalizedValue(parsedDataModel, "MccCode");
                v5 = GetNormalizedValue(parsedDataModel, "SourceAmount");
                v6 = GetNormalizedValue(parsedDataModel, "SourceCurrency");
                break;
            default:
                row.CorrelationValue = string.Empty;
                row.ReconciliationStatus = ReconciliationStatus.Failed;
                row.Message = _localizer.Get("FileIngestion.ReconciliationKeyNotGenerated", row.Message).Trim();
                return;
        }

        var anyEmpty =
            string.IsNullOrWhiteSpace(v0) || string.IsNullOrWhiteSpace(v1) ||
            string.IsNullOrWhiteSpace(v2) || string.IsNullOrWhiteSpace(v3) ||
            string.IsNullOrWhiteSpace(v4) || string.IsNullOrWhiteSpace(v5) ||
            string.IsNullOrWhiteSpace(v6);
        
        row.CorrelationValue = $"{v0}:{v1}:{v2}:{v3}:{v4}:{v5}:{v6}";

        row.ReconciliationStatus = anyEmpty
            ? ReconciliationStatus.Failed
            : ReconciliationStatus.Ready;

        if (row.ReconciliationStatus == ReconciliationStatus.Failed)
            row.Message = _localizer.Get("FileIngestion.ReconciliationKeyNotGenerated", row.Message).Trim();

        if (fileType == FileType.Clearing)
        {
            row.DuplicateDetectionKey = BuildClearingDuplicateDetectionKey(parsedDataModel);
        }
    }

    private static string? BuildClearingDuplicateDetectionKey(object parsedDataModel)
    {
        if (!TryGetLongValue(parsedDataModel, "ClrNo", out var clrNo) || clrNo <= 0)
            return null;

        var controlStat = GetStringValue(parsedDataModel, "ControlStat");
        if (string.IsNullOrWhiteSpace(controlStat))
            return null;

        return $"{clrNo.ToString(CultureInfo.InvariantCulture)}:{controlStat}";
    }

    private async Task FinalizeFileStateAsync(
        Guid transactionFileId,
        string? archiveErrorMessage,
        CancellationToken cancellationToken)
    {
        await ApplyDuplicateOutcomesAsync(transactionFileId, cancellationToken);
        await ApplyClearingMatchAsync(transactionFileId, cancellationToken);

        var metrics = await _dbContext.IngestionFileLines
            .AsNoTracking()
            .Where(x => x.FileId == transactionFileId && x.LineType == "D")
            .GroupBy(x => 1)
            .Select(group => new
            {
                TotalCount = (long)group.Count(),
                SuccessCount = (long)group.Count(x => x.Status == FileRowStatus.Success),
                ErrorCount = (long)group.Count(x => x.Status == FileRowStatus.Failed)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var file = await _dbContext.IngestionFiles
            .AsNoTracking()
            .FirstAsync(x => x.Id == transactionFileId, cancellationToken);

        var totalCount = metrics?.TotalCount ?? 0;
        var successCount = metrics?.SuccessCount ?? 0;
        var errorCount = metrics?.ErrorCount ?? 0;

        var messages = new List<string>();

        if (file.ExpectedLineCount != totalCount)
        {
            messages.Add(
                _localizer.Get("FileIngestion.ExpectedCountMismatch", file.ExpectedLineCount, totalCount));
        }

        if (errorCount > 0)
            messages.Add(_localizer.Get("FileIngestion.LinesInvalid", errorCount));

        if (!file.IsArchived)
        {
            messages.Add(string.IsNullOrWhiteSpace(archiveErrorMessage)
                ? _localizer.Get("FileIngestion.FileCopyFailed")
                : _localizer.Get("FileIngestion.FileImportedNotArchived", archiveErrorMessage));
        }

        var status = file.ExpectedLineCount == totalCount && errorCount == 0 && file.IsArchived
            ? FileStatus.Success
            : FileStatus.Failed;

        var message = messages.Count == 0
            ? _localizer.Get("FileIngestion.FileProcessedSuccessfully")
            : string.Join(" ", messages);

        var auditStamp = _auditStampService.CreateStamp();

        await _dbContext.IngestionFiles
            .Where(x => x.Id == transactionFileId)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.ProcessedLineCount, totalCount)
                    .SetProperty(x => x.SuccessfulLineCount, successCount)
                    .SetProperty(x => x.FailedLineCount, errorCount)
                    .SetProperty(x => x.Status, status)
                    .SetProperty(x => x.Message, message)
                    .SetProperty(x => x.UpdateDate, auditStamp.Timestamp)
                    .SetProperty(x => x.LastModifiedBy, auditStamp.UserId),
                cancellationToken);

        _dbContext.ChangeTracker.Clear();

        if (file.FileType == FileType.Clearing && status == FileStatus.Success)
        {
            try
            {
                var requeued = await _clearingArrivalRequeueService
                    .RequeueAwaitingCardRowsAsync(transactionFileId, cancellationToken);

                if (requeued > 0)
                {
                    _logger.LogInformation(
                        "Clearing arrival requeued {Count} awaiting-clearing rows for file {FileId}.",
                        requeued, transactionFileId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to requeue awaiting-clearing rows for clearing file {FileId}.",
                    transactionFileId);
            }
        }
    }

    private async Task ApplyDuplicateOutcomesAsync(
        Guid transactionFileId,
        CancellationToken cancellationToken)
    {
        var file = await _dbContext.IngestionFiles
            .AsNoTracking()
            .FirstAsync(x => x.Id == transactionFileId, cancellationToken);

        if (file.FileType is not (FileType.Card or FileType.Clearing))
            return;

        var auditStamp = _auditStampService.CreateStamp();
        await _dbContext.IngestionFileLines
            .Where(x => x.FileId == transactionFileId &&
                        x.LineType == "D" &&
                        x.Status == FileRowStatus.Success)
            .ExecuteUpdateAsync(update => update
                    .SetProperty(x => x.DuplicateStatus, (string?)null)
                    .SetProperty(x => x.DuplicateGroupId, (Guid?)null)
                    .SetProperty(x => x.UpdateDate, auditStamp.Timestamp)
                    .SetProperty(x => x.LastModifiedBy, auditStamp.UserId),
                cancellationToken);
        
        string? lastKey = null;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var keyPage = await _dbContext.IngestionFileLines
                .AsNoTracking()
                .Where(x => x.FileId == transactionFileId &&
                            x.LineType == "D" &&
                            x.Status == FileRowStatus.Success &&
                            x.DuplicateDetectionKey != null &&
                            (lastKey == null || x.DuplicateDetectionKey!.CompareTo(lastKey) > 0))
                .GroupBy(x => x.DuplicateDetectionKey!)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .OrderBy(k => k)
                .Take(QueryBatchSize)
                .ToListAsync(cancellationToken);

            if (keyPage.Count == 0)
                break;

            lastKey = keyPage[^1];

            var slimRows = await _dbContext.IngestionFileLines
                .AsNoTracking()
                .Where(x => x.FileId == transactionFileId &&
                            x.LineType == "D" &&
                            x.Status == FileRowStatus.Success &&
                            keyPage.Contains(x.DuplicateDetectionKey!))
                .OrderBy(x => x.LineNumber)
                .ThenBy(x => x.Id)
                .Select(x => new DuplicateRowSlim
                {
                    Id = x.Id,
                    LineNumber = x.LineNumber,
                    DuplicateDetectionKey = x.DuplicateDetectionKey!,
                    ParsedContent = x.ParsedContent ?? string.Empty,
                    OriginalReconciliationStatus = x.ReconciliationStatus,
                })
                .ToListAsync(cancellationToken);

            if (slimRows.Count == 0)
            {
                if (keyPage.Count < QueryBatchSize) break;
                continue;
            }

            foreach (var r in slimRows)
                r.ReconciliationStatus = r.OriginalReconciliationStatus;

            if (file.FileType == FileType.Card)
                ApplyCardDuplicateOutcomes(slimRows);
            else
                ApplyClearingDuplicateOutcomes(slimRows);

            await PersistDuplicateOutcomesAsync(slimRows, auditStamp, cancellationToken);

            slimRows.Clear(); // Eager release for GC

            if (keyPage.Count < QueryBatchSize)
                break;
        }

        var uniqueStatus = DuplicateStatus.Unique.ToString();
        await _dbContext.IngestionFileLines
            .Where(x => x.FileId == transactionFileId &&
                        x.LineType == "D" &&
                        x.Status == FileRowStatus.Success &&
                        x.DuplicateStatus == null)
            .ExecuteUpdateAsync(update => update
                    .SetProperty(x => x.DuplicateStatus, uniqueStatus)
                    .SetProperty(x => x.DuplicateGroupId, (Guid?)null)
                    .SetProperty(x => x.UpdateDate, auditStamp.Timestamp)
                    .SetProperty(x => x.LastModifiedBy, auditStamp.UserId),
                cancellationToken);
    }

    private async Task ApplyClearingMatchAsync(
        Guid transactionFileId,
        CancellationToken cancellationToken)
    {
        var file = await _dbContext.IngestionFiles
            .AsNoTracking()
            .FirstAsync(x => x.Id == transactionFileId, cancellationToken);

        if (file.FileType is not (FileType.Card or FileType.Clearing))
            return;

        var auditStamp = _auditStampService.CreateStamp();
        
        var (schema, table, fileTable) = ResolveLineTableNames();
        var providerName = _dbContext.Database.ProviderName ?? string.Empty;
        var isPostgres = providerName.Contains("Npgsql", StringComparison.OrdinalIgnoreCase);

        var (lineCols, fileCols) = ResolveLineAndFileColumnNames();

        if (file.FileType == FileType.Clearing)
        {
            var sql = isPostgres
                ? BuildPostgresClearingToCardMatchSql(schema, table, fileTable, lineCols, fileCols)
                : BuildSqlServerClearingToCardMatchSql(schema, table, fileTable, lineCols, fileCols);

            await _dbContext.Database.ExecuteSqlRawAsync(
                sql,
                new object[]
                {
                    transactionFileId,
                    FileRowStatus.Success.ToString(),
                    FileType.Card.ToString(),
                    auditStamp.Timestamp,
                    (object?)auditStamp.UserId ?? DBNull.Value
                },
                cancellationToken);
        }
        else // FileType.Card
        {
            var sql = isPostgres
                ? BuildPostgresCardToClearingMatchSql(schema, table, fileTable, lineCols, fileCols)
                : BuildSqlServerCardToClearingMatchSql(schema, table, fileTable, lineCols, fileCols);

            await _dbContext.Database.ExecuteSqlRawAsync(
                sql,
                new object[]
                {
                    transactionFileId,
                    FileRowStatus.Success.ToString(),
                    FileType.Clearing.ToString(),
                    auditStamp.Timestamp,
                    (object?)auditStamp.UserId ?? DBNull.Value
                },
                cancellationToken);
        }

        _dbContext.ChangeTracker.Clear();
    }
    
    private (string Schema, string LineTable, string FileTable) ResolveLineTableNames()
    {
        if (_cachedLineTableNames is { } cached)
            return cached;

        lock (_modelCacheLock)
        {
            if (_cachedLineTableNames is { } cachedInner)
                return cachedInner;

            var lineEt = _dbContext.Model.FindEntityType(typeof(IngestionFileLineEntity))!;
            var fileEt = _dbContext.Model.FindEntityType(typeof(IngestionFileEntity))!;
            var result = (lineEt.GetSchema() ?? string.Empty, lineEt.GetTableName()!, fileEt.GetTableName()!);
            _cachedLineTableNames = result;
            return result;
        }
    }

    private (Dictionary<string, string> LineCols, Dictionary<string, string> FileCols) ResolveLineAndFileColumnNames()
    {
        if (_cachedColumnNames is { } cached)
            return cached;

        lock (_modelCacheLock)
        {
            if (_cachedColumnNames is { } cachedInner)
                return cachedInner;

            var lineEt = _dbContext.Model.FindEntityType(typeof(IngestionFileLineEntity))!;
            var lineSo = StoreObjectIdentifier.Table(lineEt.GetTableName()!, lineEt.GetSchema());
            var lineCols = lineEt.GetProperties().ToDictionary(p => p.Name, p => p.GetColumnName(lineSo)!);

            var fileEt = _dbContext.Model.FindEntityType(typeof(IngestionFileEntity))!;
            var fileSo = StoreObjectIdentifier.Table(fileEt.GetTableName()!, fileEt.GetSchema());
            var fileCols = fileEt.GetProperties().ToDictionary(p => p.Name, p => p.GetColumnName(fileSo)!);

            var result = (lineCols, fileCols);
            _cachedColumnNames = result;
            return result;
        }
    }

    private static string Quote(bool isPostgres, string identifier)
        => isPostgres ? $"\"{identifier}\"" : $"[{identifier}]";

    private static string QualifiedTable(bool isPostgres, string schema, string table)
    {
        if (string.IsNullOrEmpty(schema))
            return Quote(isPostgres, table);
        return $"{Quote(isPostgres, schema)}.{Quote(isPostgres, table)}";
    }

    private static string BuildPostgresClearingToCardMatchSql(
        string schema, string lineTable, string fileTable,
        Dictionary<string, string> lc, Dictionary<string, string> fc)
    {
        var fl = QualifiedTable(true, schema, lineTable);
        var ff = QualifiedTable(true, schema, fileTable);
        return $@"
WITH src AS (
    SELECT DISTINCT ON (""{lc[nameof(IngestionFileLineEntity.CorrelationKey)]}"",
                        ""{lc[nameof(IngestionFileLineEntity.CorrelationValue)]}"")
           ""{lc[nameof(IngestionFileLineEntity.CorrelationKey)]}"" AS ck,
           ""{lc[nameof(IngestionFileLineEntity.CorrelationValue)]}"" AS cv,
           ""{lc[nameof(IngestionFileLineEntity.Id)]}"" AS clr_id
    FROM {fl}
    WHERE ""{lc[nameof(IngestionFileLineEntity.FileId)]}"" = @p0
      AND ""{lc[nameof(IngestionFileLineEntity.LineType)]}"" = 'D'
      AND ""{lc[nameof(IngestionFileLineEntity.Status)]}"" = @p1
      AND ""{lc[nameof(IngestionFileLineEntity.CorrelationKey)]}"" IS NOT NULL
      AND ""{lc[nameof(IngestionFileLineEntity.CorrelationValue)]}"" IS NOT NULL
    ORDER BY ""{lc[nameof(IngestionFileLineEntity.CorrelationKey)]}"",
             ""{lc[nameof(IngestionFileLineEntity.CorrelationValue)]}"",
             ""{lc[nameof(IngestionFileLineEntity.Id)]}"" DESC
)
UPDATE {fl} card
SET ""{lc[nameof(IngestionFileLineEntity.MatchedClearingLineId)]}"" = src.clr_id,
    ""{lc[nameof(IngestionFileLineEntity.UpdateDate)]}"" = @p3,
    ""{lc[nameof(IngestionFileLineEntity.LastModifiedBy)]}"" = @p4
FROM src, {ff} cf
WHERE cf.""{fc[nameof(IngestionFileEntity.Id)]}"" = card.""{lc[nameof(IngestionFileLineEntity.FileId)]}""
  AND card.""{lc[nameof(IngestionFileLineEntity.LineType)]}"" = 'D'
  AND cf.""{fc[nameof(IngestionFileEntity.FileType)]}"" = @p2
  AND card.""{lc[nameof(IngestionFileLineEntity.CorrelationKey)]}"" = src.ck
  AND card.""{lc[nameof(IngestionFileLineEntity.CorrelationValue)]}"" = src.cv;
";
    }

    private static string BuildSqlServerClearingToCardMatchSql(
        string schema, string lineTable, string fileTable,
        Dictionary<string, string> lc, Dictionary<string, string> fc)
    {
        var fl = QualifiedTable(false, schema, lineTable);
        var ff = QualifiedTable(false, schema, fileTable);
        return $@"
WITH src AS (
    SELECT [{lc[nameof(IngestionFileLineEntity.CorrelationKey)]}] AS ck,
           [{lc[nameof(IngestionFileLineEntity.CorrelationValue)]}] AS cv,
           MAX([{lc[nameof(IngestionFileLineEntity.Id)]}]) AS clr_id
    FROM {fl}
    WHERE [{lc[nameof(IngestionFileLineEntity.FileId)]}] = @p0
      AND [{lc[nameof(IngestionFileLineEntity.LineType)]}] = 'D'
      AND [{lc[nameof(IngestionFileLineEntity.Status)]}] = @p1
      AND [{lc[nameof(IngestionFileLineEntity.CorrelationKey)]}] IS NOT NULL
      AND [{lc[nameof(IngestionFileLineEntity.CorrelationValue)]}] IS NOT NULL
    GROUP BY [{lc[nameof(IngestionFileLineEntity.CorrelationKey)]}],
             [{lc[nameof(IngestionFileLineEntity.CorrelationValue)]}]
)
UPDATE card
SET [{lc[nameof(IngestionFileLineEntity.MatchedClearingLineId)]}] = src.clr_id,
    [{lc[nameof(IngestionFileLineEntity.UpdateDate)]}] = @p3,
    [{lc[nameof(IngestionFileLineEntity.LastModifiedBy)]}] = @p4
FROM {fl} card
INNER JOIN src ON card.[{lc[nameof(IngestionFileLineEntity.CorrelationKey)]}] = src.ck
              AND card.[{lc[nameof(IngestionFileLineEntity.CorrelationValue)]}] = src.cv
INNER JOIN {ff} cf ON cf.[{fc[nameof(IngestionFileEntity.Id)]}] = card.[{lc[nameof(IngestionFileLineEntity.FileId)]}]
WHERE card.[{lc[nameof(IngestionFileLineEntity.LineType)]}] = 'D'
  AND cf.[{fc[nameof(IngestionFileEntity.FileType)]}] = @p2;
";
    }

    private static string BuildPostgresCardToClearingMatchSql(
        string schema, string lineTable, string fileTable,
        Dictionary<string, string> lc, Dictionary<string, string> fc)
    {
        var fl = QualifiedTable(true, schema, lineTable);
        var ff = QualifiedTable(true, schema, fileTable);
        return $@"
WITH src AS (
    SELECT DISTINCT ON (clr.""{lc[nameof(IngestionFileLineEntity.CorrelationKey)]}"",
                        clr.""{lc[nameof(IngestionFileLineEntity.CorrelationValue)]}"")
           clr.""{lc[nameof(IngestionFileLineEntity.CorrelationKey)]}"" AS ck,
           clr.""{lc[nameof(IngestionFileLineEntity.CorrelationValue)]}"" AS cv,
           clr.""{lc[nameof(IngestionFileLineEntity.Id)]}"" AS clr_id
    FROM {fl} clr
    JOIN {ff} cf ON cf.""{fc[nameof(IngestionFileEntity.Id)]}"" = clr.""{lc[nameof(IngestionFileLineEntity.FileId)]}""
    WHERE clr.""{lc[nameof(IngestionFileLineEntity.LineType)]}"" = 'D'
      AND cf.""{fc[nameof(IngestionFileEntity.FileType)]}"" = @p2
    ORDER BY clr.""{lc[nameof(IngestionFileLineEntity.CorrelationKey)]}"",
             clr.""{lc[nameof(IngestionFileLineEntity.CorrelationValue)]}"",
             clr.""{lc[nameof(IngestionFileLineEntity.CreateDate)]}"" DESC,
             clr.""{lc[nameof(IngestionFileLineEntity.Id)]}"" DESC
)
UPDATE {fl} card
SET ""{lc[nameof(IngestionFileLineEntity.MatchedClearingLineId)]}"" = src.clr_id,
    ""{lc[nameof(IngestionFileLineEntity.UpdateDate)]}"" = @p3,
    ""{lc[nameof(IngestionFileLineEntity.LastModifiedBy)]}"" = @p4
FROM src
WHERE card.""{lc[nameof(IngestionFileLineEntity.FileId)]}"" = @p0
  AND card.""{lc[nameof(IngestionFileLineEntity.LineType)]}"" = 'D'
  AND card.""{lc[nameof(IngestionFileLineEntity.Status)]}"" = @p1
  AND card.""{lc[nameof(IngestionFileLineEntity.CorrelationKey)]}"" = src.ck
  AND card.""{lc[nameof(IngestionFileLineEntity.CorrelationValue)]}"" = src.cv;
";
    }

    private static string BuildSqlServerCardToClearingMatchSql(
        string schema, string lineTable, string fileTable,
        Dictionary<string, string> lc, Dictionary<string, string> fc)
    {
        var fl = QualifiedTable(false, schema, lineTable);
        var ff = QualifiedTable(false, schema, fileTable);
        return $@"
WITH src AS (
    SELECT clr.[{lc[nameof(IngestionFileLineEntity.CorrelationKey)]}] AS ck,
           clr.[{lc[nameof(IngestionFileLineEntity.CorrelationValue)]}] AS cv,
           clr.[{lc[nameof(IngestionFileLineEntity.Id)]}] AS clr_id,
           ROW_NUMBER() OVER (
               PARTITION BY clr.[{lc[nameof(IngestionFileLineEntity.CorrelationKey)]}],
                            clr.[{lc[nameof(IngestionFileLineEntity.CorrelationValue)]}]
               ORDER BY clr.[{lc[nameof(IngestionFileLineEntity.CreateDate)]}] DESC,
                        clr.[{lc[nameof(IngestionFileLineEntity.Id)]}] DESC) AS rn
    FROM {fl} clr
    INNER JOIN {ff} cf ON cf.[{fc[nameof(IngestionFileEntity.Id)]}] = clr.[{lc[nameof(IngestionFileLineEntity.FileId)]}]
    WHERE clr.[{lc[nameof(IngestionFileLineEntity.LineType)]}] = 'D'
      AND cf.[{fc[nameof(IngestionFileEntity.FileType)]}] = @p2
)
UPDATE card
SET [{lc[nameof(IngestionFileLineEntity.MatchedClearingLineId)]}] = src.clr_id,
    [{lc[nameof(IngestionFileLineEntity.UpdateDate)]}] = @p3,
    [{lc[nameof(IngestionFileLineEntity.LastModifiedBy)]}] = @p4
FROM {fl} card
INNER JOIN src ON src.rn = 1
              AND card.[{lc[nameof(IngestionFileLineEntity.CorrelationKey)]}] = src.ck
              AND card.[{lc[nameof(IngestionFileLineEntity.CorrelationValue)]}] = src.cv
WHERE card.[{lc[nameof(IngestionFileLineEntity.FileId)]}] = @p0
  AND card.[{lc[nameof(IngestionFileLineEntity.LineType)]}] = 'D'
  AND card.[{lc[nameof(IngestionFileLineEntity.Status)]}] = @p1;
";
    }
    
    private sealed class DuplicateRowSlim
    {
        public Guid Id { get; init; }
        public long LineNumber { get; init; }
        public string DuplicateDetectionKey { get; init; } = string.Empty;
        public string ParsedContent { get; init; } = string.Empty;
        public ReconciliationStatus? OriginalReconciliationStatus { get; init; }
        public string? DuplicateStatus { get; set; }
        public Guid? DuplicateGroupId { get; set; }
        public ReconciliationStatus? ReconciliationStatus { get; set; }
    }

    private async Task PersistDuplicateOutcomesAsync(
        List<DuplicateRowSlim> rows,
        AuditStamp auditStamp,
        CancellationToken cancellationToken)
    {
        var buckets = rows
            .Where(r => r.DuplicateStatus is not null && r.DuplicateGroupId is not null)
            .GroupBy(r => (r.DuplicateStatus, r.DuplicateGroupId, r.ReconciliationStatus));

        foreach (var bucket in buckets)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var (dupStatus, dupGroupId, reconStatus) = bucket.Key;
            var ids = bucket.Select(r => r.Id).ToArray();
            if (ids.Length == 0) continue;
            
            foreach (var idBatch in Batch(ids, 1000))
            {
                await _dbContext.IngestionFileLines
                    .Where(x => idBatch.Contains(x.Id))
                    .ExecuteUpdateAsync(update => update
                            .SetProperty(x => x.DuplicateStatus, dupStatus)
                            .SetProperty(x => x.DuplicateGroupId, dupGroupId)
                            .SetProperty(x => x.ReconciliationStatus, reconStatus)
                            .SetProperty(x => x.UpdateDate, auditStamp.Timestamp)
                            .SetProperty(x => x.LastModifiedBy, auditStamp.UserId),
                        cancellationToken);
            }
        }
    }

    private static void ApplyCardDuplicateOutcomes(List<DuplicateRowSlim> detailRows)
    {
        ProcessDuplicateGroups(detailRows, isCard: true);
    }

    private static void ApplyClearingDuplicateOutcomes(List<DuplicateRowSlim> detailRows)
    {
        ProcessDuplicateGroups(detailRows, isCard: false);
    }

    private static void ProcessDuplicateGroups(List<DuplicateRowSlim> detailRows, bool isCard)
    {
        var withKey = new List<DuplicateRowSlim>(detailRows.Count);
        foreach (var r in detailRows)
            if (!string.IsNullOrWhiteSpace(r.DuplicateDetectionKey))
                withKey.Add(r);

        if (withKey.Count < 2) return;

        withKey.Sort(static (a, b) => string.CompareOrdinal(a.DuplicateDetectionKey, b.DuplicateDetectionKey));

        var i = 0;
        while (i < withKey.Count)
        {
            var key = withKey[i].DuplicateDetectionKey;
            var j = i + 1;
            while (j < withKey.Count && string.Equals(withKey[j].DuplicateDetectionKey, key, StringComparison.Ordinal))
                j++;

            var groupCount = j - i;
            if (groupCount > 1)
            {
                var primary = withKey[i];
                var allEquivalent = true;

                if (isCard)
                {
                    var primarySig = BuildCardDuplicateSignature(primary.ParsedContent);
                    for (var k = i + 1; k < j; k++)
                    {
                        var sig = BuildCardDuplicateSignature(withKey[k].ParsedContent);
                        if (!CardDuplicateSignatureEquals(primarySig, sig))
                        {
                            allEquivalent = false;
                            break;
                        }
                    }
                }
                else
                {
                    var primaryPayload = primary.ParsedContent;
                    for (var k = i + 1; k < j; k++)
                    {
                        if (!string.Equals(withKey[k].ParsedContent, primaryPayload, StringComparison.Ordinal))
                        {
                            allEquivalent = false;
                            break;
                        }
                    }
                }
                
                var groupRows = withKey.GetRange(i, groupCount);
                if (allEquivalent)
                    MarkEquivalentDuplicateGroup(groupRows, primary);
                else
                    MarkConflictingDuplicateGroup(groupRows);
            }

            i = j;
        }
    }

    private static void MarkEquivalentDuplicateGroup(
        List<DuplicateRowSlim> rows,
        DuplicateRowSlim primary)
    {
        var duplicateGroupId = Guid.NewGuid();
        primary.DuplicateStatus = LinkPara.Card.Domain.Enums.FileIngestion.DuplicateStatus.Primary.ToString();
        primary.DuplicateGroupId = duplicateGroupId;
        primary.ReconciliationStatus ??= ReconciliationStatus.Ready;

        foreach (var secondary in rows)
        {
            if (ReferenceEquals(secondary, primary)) continue;
            secondary.DuplicateStatus = LinkPara.Card.Domain.Enums.FileIngestion.DuplicateStatus.Secondary.ToString();
            secondary.DuplicateGroupId = duplicateGroupId;
            secondary.ReconciliationStatus = ReconciliationStatus.Failed;
        }
    }

    private static void MarkConflictingDuplicateGroup(
        IEnumerable<DuplicateRowSlim> rows)
    {
        var duplicateGroupId = Guid.NewGuid();
        foreach (var row in rows)
        {
            row.DuplicateStatus = LinkPara.Card.Domain.Enums.FileIngestion.DuplicateStatus.Conflict.ToString();
            row.DuplicateGroupId = duplicateGroupId;
            row.ReconciliationStatus = ReconciliationStatus.Failed;
        }
    }

    private static CardDuplicateSignature BuildCardDuplicateSignature(string parsedData)
    {
        using var document = JsonDocument.Parse(parsedData);
        var root = document.RootElement;

        return new CardDuplicateSignature(
            GetString(root, "CardNo"),
            GetInt(root, "Otc"),
            GetInt(root, "Ots"),
            GetDecimal(root, "CardHolderBillingAmount"));
    }

    private static bool CardDuplicateSignatureEquals(CardDuplicateSignature left, CardDuplicateSignature right)
    {
        return string.Equals(left.CardNo, right.CardNo, StringComparison.Ordinal) &&
               left.Otc == right.Otc &&
               left.Ots == right.Ots &&
               left.CardHolderBillingAmount == right.CardHolderBillingAmount;
    }

    private static string GetString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) && value.ValueKind != JsonValueKind.Null
            ? value.ToString()
            : string.Empty;
    }

    private static int GetInt(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value) ||
            value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            return 0;

        return value.ValueKind switch
        {
            JsonValueKind.Number when value.TryGetInt32(out var parsed) => parsed,
            JsonValueKind.String when int.TryParse(value.GetString(), out var parsed) => parsed,
            _ => 0
        };
    }

    private static decimal GetDecimal(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value) ||
            value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            return 0m;

        return value.ValueKind switch
        {
            JsonValueKind.Number when value.TryGetDecimal(out var parsed) => parsed,
            JsonValueKind.String when decimal.TryParse(value.GetString(), out var parsed) => parsed,
            _ => 0m
        };
    }

    private readonly record struct CardDuplicateSignature(
        string CardNo,
        int Otc,
        int Ots,
        decimal CardHolderBillingAmount);


    private async Task UpdateFileMessageAsync(
        Guid transactionFileId,
        string message,
        CancellationToken cancellationToken)
    {
        var auditStamp = _auditStampService.CreateStamp();
        await _dbContext.IngestionFiles
            .Where(x => x.Id == transactionFileId)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.Message, message)
                    .SetProperty(x => x.UpdateDate, auditStamp.Timestamp)
                    .SetProperty(x => x.LastModifiedBy, auditStamp.UserId),
                cancellationToken);
    }

    private async Task<IngestionFileEntity> GetFileAsync(
        Guid transactionFileId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.IngestionFiles
            .AsTracking()
            .FirstAsync(x => x.Id == transactionFileId, cancellationToken);
    }

    private async Task<(BoundaryRecord Header, BoundaryRecord Footer)> ReadBoundaryRecordsAsync(
        FileReference file,
        string profileKey,
        ParsingOptions parsingRule,
        IFileTransferClient transferClient,
        Encoding encoding,
        CancellationToken cancellationToken)
    {
        try
        {
            string headerLine;
            try
            {
                await using var headerStream = await transferClient.OpenReadAsync(file, cancellationToken);
                headerLine = await ReadFirstMatchingLineAsync(headerStream, encoding, parsingRule.HeaderPrefix,
                    cancellationToken);
            }
            catch (FileIngestionRecordNotFoundFromStartException)
            {
                throw new FileIngestionHeaderNotResolvedException(_localizer.Get("FileIngestion.HeaderNotResolved",
                    file.Name));
            }

            string footerLine;
            try
            {
                await using var footerStream = await transferClient.OpenReadAsync(file, cancellationToken);
                footerLine = await ReadLastMatchingLineAsync(footerStream, encoding, parsingRule.FooterPrefix,
                    cancellationToken);
            }
            catch (FileIngestionRecordNotFoundFromEndException)
            {
                throw new FileIngestionFooterNotResolvedException(_localizer.Get("FileIngestion.FooterNotResolved",
                    file.Name));
            }

            return (
                ParseBoundaryRecord(profileKey, parsingRule, headerLine, parsingRule.HeaderPrefix),
                ParseBoundaryRecord(profileKey, parsingRule, footerLine, parsingRule.FooterPrefix));
        }
        catch (FileIngestionHeaderNotResolvedException)
        {
            throw;
        }
        catch (FileIngestionFooterNotResolvedException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new FileIngestionBoundaryRecordReadFailedException(
                _localizer.Get("FileIngestion.BoundaryRecordReadFailed", file.Name, ex.Message), ex);
        }
    }

    private BoundaryRecord ParseBoundaryRecord(
        string profileKey,
        ParsingOptions parsingRule,
        string line,
        string expectedRecordType)
    {
        if (string.IsNullOrWhiteSpace(line))
            throw new FileIngestionBoundaryRecordEmptyException(_localizer.Get("FileIngestion.BoundaryRecordEmpty",
                expectedRecordType));

        var parsed = _fixedWidthRecordParser.Parse(line, parsingRule);
        if (!string.Equals(parsed.RecordType, expectedRecordType, StringComparison.OrdinalIgnoreCase))
        {
            throw new FileIngestionRecordTypeMismatchException(
                _localizer.Get("FileIngestion.RecordTypeMismatch", expectedRecordType, parsed.RecordType));
        }

        parsed.ParsedDataModel = _parsedRecordModelMapper.Create(profileKey, parsed);

        return new BoundaryRecord
        {
            RawLine = line,
            ParsedLine = parsed,
            TypedModel = parsed.ParsedDataModel
        };
    }

    private static string GenerateFileKey(object headerModel, object footerModel)
    {
        var normalized = string.Join("|", new[]
        {
            BuildFileKeySegment("H", headerModel),
            BuildFileKeySegment("F", footerModel)
        });

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(normalized)));
    }

    private string GetRequiredProperty(object instance, string propertyName)
    {
        var property = instance.GetType().GetProperty(propertyName)
                       ?? throw new FileIngestionPropertyNotDefinedException(
                           _localizer.Get("FileIngestion.PropertyNotDefined", propertyName, instance.GetType().Name));

        var value = property.GetValue(instance);
        var stringValue = Convert.ToString(value, CultureInfo.InvariantCulture);

        if (string.IsNullOrWhiteSpace(stringValue))
            throw new FileIngestionPropertyEmptyException(_localizer.Get("FileIngestion.PropertyEmpty", propertyName,
                instance.GetType().Name));

        return stringValue;
    }

    private static string BuildFileKeySegment(string prefix, object model)
    {
        var values = model.GetType()
            .GetProperties()
            .Where(x => x.CanRead && x.GetIndexParameters().Length == 0)
            .OrderBy(x => x.Name, StringComparer.Ordinal)
            .Select(x =>
            {
                var value = x.GetValue(model);
                var stringValue = Convert.ToString(value, CultureInfo.InvariantCulture)?.Trim() ?? string.Empty;
                return $"{x.Name}={stringValue}";
            });

        return $"{prefix}:{string.Join(";", values)}";
    }

    private long GetFooterTxnCount(object footerModel)
    {
        var property = footerModel.GetType().GetProperty("TxnCount")
                       ?? throw new FileIngestionFooterTxnCountMissingException(
                           _localizer.Get("FileIngestion.FooterTxnCountMissing", footerModel.GetType().Name));

        return Convert.ToInt64(property.GetValue(footerModel), CultureInfo.InvariantCulture);
    }

    private async Task<TargetWriter> TryOpenTargetWriterAsync(
        string profileKey,
        string fileName,
        CancellationToken cancellationToken)
    {
        try
        {
            var targetTransferClient = _fileTransferClientResolver.Create(
                ResolveTargetSourceType(),
                FileTransferEndpointType.Target);

            var targetStream = await targetTransferClient.OpenWriteAsync(
                profileKey,
                fileName,
                FileTransferEndpointType.Target,
                cancellationToken);

            return new TargetWriter(targetStream, _logger, _localizer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                _localizer.Get("FileIngestion.TargetArchiveOpenFailed"),
                profileKey, fileName, ex.Message);
            var detailedMessage = $"[{ex.GetType().Name}] {ex.Message}";
            if (ex.InnerException != null)
                detailedMessage += $" -> [{ex.InnerException.GetType().Name}] {ex.InnerException.Message}";
            return new TargetWriter(null, _logger, _localizer, detailedMessage);
        }
    }

    private async Task<IReadOnlyCollection<FileReference>> ResolveFilesAsync(
        FileIngestionRequest request,
        string profileKey,
        ProfileOptions profile,
        IFileTransferClient transferClient,
        CancellationToken cancellationToken)
    {
        if (request.FileSourceType == FileSourceType.Local && !string.IsNullOrWhiteSpace(request.FilePath))
        {
            if (Directory.Exists(request.FilePath))
            {
                var matches = Directory.EnumerateFiles(request.FilePath, "*", SearchOption.TopDirectoryOnly)
                    .Select(path => new { Path = path, Name = Path.GetFileName(path) })
                    .Where(file => MatchesProfileFileName(profile, file.Name))
                    .OrderBy(x => x.Name, FileReferenceNameComparer.Instance)
                    .Select(x => new FileReference
                    {
                        Name = x.Name,
                        FullPath = x.Path
                    })
                    .ToArray();

                return matches;
            }

            var explicitFileName = Path.GetFileName(request.FilePath);
            if (!MatchesProfileFileName(profile, explicitFileName))
                throw new FileIngestionFilePatternMismatchException(_localizer.Get("FileIngestion.FilePatternMismatch",
                    explicitFileName));

            return
            [
                new FileReference
                {
                    Name = explicitFileName,
                    FullPath = request.FilePath
                }
            ];
        }

        return await transferClient.ListAsync(
            profileKey,
            profile,
            FileTransferEndpointType.Source,
            cancellationToken);
    }

    private ProfileOptions GetProfile(string profileKey)
    {
        if (!_options.Profiles.TryGetValue(profileKey, out var profile))
            throw new FileIngestionProfileNotConfiguredException(_localizer.Get("FileIngestion.ProfileNotConfigured",
                profileKey));

        return profile;
    }

    private ParsingOptions GetParsingRule(ProfileOptions profile)
    {
        return profile.Parsing
               ?? throw new FileIngestionParsingNotDefinedException(_localizer.Get("FileIngestion.ParsingNotDefined"));
    }

    private int GetBatchSize() => Math.Max(1, _options.Processing.BatchSize ?? 1);

    private int GetRetryBatchSize() => Math.Max(1, _options.Processing.RetryBatchSize ?? 1);

    private int GetFailedRowMaxRetryCount() => Math.Max(1, _options.Processing.FailedRowMaxRetryCount ?? 1);

    private int GetMaxDegreeOfParallelism() => Math.Max(1, _options.Processing.MaxDegreeOfParallelism ?? 1);

    private static string BuildProfileKey(FileType fileType, FileContentType contentType) => $"{fileType}{contentType}";

    private static FileType GetFileTypeFromProfileKey(string profileKey)
        => profileKey.StartsWith(nameof(FileType.Clearing), StringComparison.OrdinalIgnoreCase)
            ? FileType.Clearing
            : FileType.Card;

    private static Encoding ResolveEncoding(string encodingName)
        => Encoding.GetEncoding(string.IsNullOrWhiteSpace(encodingName) ? "UTF-8" : encodingName);

    private static bool MatchesProfileFileName(ProfileOptions profile, string fileName)
    {
        if (_profileRegexByInstance.TryGetValue(profile, out var cachedRegex))
            return cachedRegex.IsMatch(fileName);

        var allowedExtensions = new HashSet<string>(
            (profile.FileExtensions ?? Enumerable.Empty<string>())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!.Trim())
            .Select(x => x.StartsWith('.') ? x : "." + x),
            StringComparer.OrdinalIgnoreCase);

        var pattern = profile.Pattern ?? string.Empty;
        if (pattern.EndsWith("$", StringComparison.Ordinal))
            pattern = pattern[..^1];
        
        var sortedExtensions = allowedExtensions
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToArray();
        var cacheKey = pattern + "||" + string.Join(",", sortedExtensions);

        var regex = _profileRegexCache.GetOrAdd(cacheKey, _ =>
        {
            string finalPattern;
            if (sortedExtensions.Length > 0)
            {
                var extensionPattern = string.Join("|", sortedExtensions.Select(Regex.Escape));
                finalPattern = $"{pattern}(?:{extensionPattern})$";
            }
            else
            {
                finalPattern = $"{pattern}$";
            }

            return new Regex(finalPattern,
                RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        });
        
        try
        {
            _profileRegexByInstance.Add(profile, regex);
        }
        catch (ArgumentException)
        {
            /* already added by another thread */
        }

        return regex.IsMatch(fileName);
    }

    private static string ResolveBoundaryReadErrorCode(Exception ex)
    {
        var apiException = ex as ApiException ?? ex.InnerException as ApiException;
        return apiException?.Code switch
        {
            ApiErrorCode.FileIngestionHeaderNotResolved => "HEADER_NOT_FOUND",
            ApiErrorCode.FileIngestionFooterNotResolved => "FOOTER_NOT_FOUND",
            _ => "BOUNDARY_READ_FAILED"
        };
    }

    private static bool RequiresArchiveOnlyRecovery(IngestionFileEntity file)
        => file.ExpectedLineCount == file.ProcessedLineCount && file.FailedLineCount == 0 && !file.IsArchived;

    private static bool RequiresFullRecovery(IngestionFileEntity file)
        => file.ExpectedLineCount != file.ProcessedLineCount || file.FailedLineCount > 0;

    private FileSourceType ResolveTargetSourceType()
    {
        return string.Equals(_options.Connections.Target.Protocol, "Local", StringComparison.OrdinalIgnoreCase)
            ? FileSourceType.Local
            : FileSourceType.Remote;
    }
    
    private static readonly ConcurrentDictionary<(Type, string), Func<object, object?>?> _modelGetterCache = new();

    private static Func<object, object?>? GetModelGetter(Type modelType, string propertyName)
    {
        return _modelGetterCache.GetOrAdd((modelType, propertyName), static k =>
        {
            var prop = k.Item1.GetProperty(k.Item2);
            if (prop is null)
                return null;

            var p = Expression.Parameter(typeof(object), "m");
            var cast = Expression.Convert(p, k.Item1);
            var access = Expression.Property(cast, prop);
            var box = Expression.Convert(access, typeof(object));
            return Expression.Lambda<Func<object, object?>>(box, p).Compile();
        });
    }

    private static string GetStringValue(object model, string propertyName)
    {
        var getter = GetModelGetter(model.GetType(), propertyName);
        if (getter is null)
            return string.Empty;

        return Convert.ToString(getter(model), CultureInfo.InvariantCulture)?.Trim() ?? string.Empty;
    }

    private static string GetNormalizedValue(object model, string propertyName)
    {
        var getter = GetModelGetter(model.GetType(), propertyName);
        if (getter is null)
            return string.Empty;

        var value = getter(model);

        return value switch
        {
            null => string.Empty,
            decimal decimalValue => decimalValue.ToString(CultureInfo.InvariantCulture),
            double doubleValue => doubleValue.ToString(CultureInfo.InvariantCulture),
            float floatValue => floatValue.ToString(CultureInfo.InvariantCulture),
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString()?.Trim() ?? string.Empty
        };
    }

    private static bool TryGetLongValue(object model, string propertyName, out long value)
    {
        value = 0;

        var getter = GetModelGetter(model.GetType(), propertyName);
        if (getter is null)
            return false;

        var rawValue = getter(model);
        if (rawValue is null)
            return false;

        if (rawValue is long longValue)
        {
            value = longValue;
            return true;
        }

        return long.TryParse(
            Convert.ToString(rawValue, CultureInfo.InvariantCulture),
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out value);
    }

    private static async Task SkipToOffsetAsync(
        Stream stream,
        long byteOffset,
        CancellationToken cancellationToken)
    {
        if (byteOffset <= 0)
            return;

        if (stream.CanSeek)
        {
            stream.Seek(byteOffset, SeekOrigin.Begin);
            return;
        }

        var buffer = ArrayPool<byte>.Shared.Rent(65_536);

        try
        {
            long skipped = 0;

            while (skipped < byteOffset)
            {
                var bytesToRead = (int)Math.Min(buffer.Length, byteOffset - skipped);
                var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, bytesToRead), cancellationToken);
                if (bytesRead == 0)
                    break;

                skipped += bytesRead;
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
    
    private static async Task ReadLinesWithByteOffsetsAsync(
        Stream stream,
        Encoding encoding,
        Func<FileLineReadResult, Task> onLineAsync,
        Func<ReadOnlyMemory<byte>, CancellationToken, Task>? onBytesAsync,
        CancellationToken cancellationToken = default,
        long startingLineNumber = 0,
        long startingByteOffset = 0)
    {
        const int ioBufferSize = 128 * 1024;
        const int initialLineBufferSize = 4 * 1024;

        var preamble = encoding.GetPreamble();
        if (preamble.Length > 0 && stream.CanSeek && stream.Position == 0 && stream.Length >= preamble.Length)
        {
            var preambleBuffer = new byte[preamble.Length];
            var preambleRead = await stream.ReadAsync(preambleBuffer.AsMemory(0, preamble.Length), cancellationToken);

            if (preambleRead == preamble.Length && preambleBuffer.AsSpan().SequenceEqual(preamble))
            {
                if (onBytesAsync is not null)
                    await onBytesAsync(preambleBuffer.AsMemory(0, preamble.Length), cancellationToken);
            }
            else
            {
                stream.Seek(0, SeekOrigin.Begin);
            }
        }

        var buffer = ArrayPool<byte>.Shared.Rent(ioBufferSize);
        var lineBuffer = ArrayPool<byte>.Shared.Rent(initialLineBufferSize);
        var lineLen = 0;

        var lineNumber = startingLineNumber;
        var lineStartOffset = stream.CanSeek ? stream.Position : startingByteOffset;
        var currentOffset = lineStartOffset;

        try
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
                if (bytesRead == 0)
                    break;

                if (onBytesAsync is not null)
                    await onBytesAsync(buffer.AsMemory(0, bytesRead), cancellationToken);

                var pos = 0;
                while (pos < bytesRead)
                {
                    var remaining = bytesRead - pos;
                    var nlAbs = Array.IndexOf(buffer, (byte)'\n', pos, remaining);

                    if (nlAbs < 0)
                    {
                        EnsureLineBufferCapacity(ref lineBuffer, lineLen, remaining);
                        Buffer.BlockCopy(buffer, pos, lineBuffer, lineLen, remaining);
                        lineLen += remaining;
                        currentOffset += remaining;
                        break;
                    }

                    var lineSegmentLen = nlAbs - pos;
                    if (lineSegmentLen > 0)
                    {
                        EnsureLineBufferCapacity(ref lineBuffer, lineLen, lineSegmentLen);
                        Buffer.BlockCopy(buffer, pos, lineBuffer, lineLen, lineSegmentLen);
                        lineLen += lineSegmentLen;
                    }

                    currentOffset += lineSegmentLen + 1;
                    await EmitLineAsync(lineBuffer, lineLen, encoding, onLineAsync, ++lineNumber, lineStartOffset,
                        currentOffset);
                    lineLen = 0;
                    lineStartOffset = currentOffset;
                    pos = nlAbs + 1;
                }
            }

            if (lineLen > 0)
                await EmitLineAsync(lineBuffer, lineLen, encoding, onLineAsync, ++lineNumber, lineStartOffset,
                    currentOffset);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
            ArrayPool<byte>.Shared.Return(lineBuffer);
        }
    }

    private static void EnsureLineBufferCapacity(ref byte[] lineBuffer, int currentLen, int additional)
    {
        var required = currentLen + additional;
        if (required <= lineBuffer.Length)
            return;

        var newSize = lineBuffer.Length;
        while (newSize < required)
            newSize *= 2;

        var newBuffer = ArrayPool<byte>.Shared.Rent(newSize);
        if (currentLen > 0)
            Buffer.BlockCopy(lineBuffer, 0, newBuffer, 0, currentLen);
        ArrayPool<byte>.Shared.Return(lineBuffer);
        lineBuffer = newBuffer;
    }

    private static async Task EmitLineAsync(
        byte[] lineBuffer,
        int lineLen,
        Encoding encoding,
        Func<FileLineReadResult, Task> onLineAsync,
        long lineNumber,
        long lineStartOffset,
        long nextByteOffset)
    {
        var (effectiveLen, lineString) = DecodeLine(lineBuffer, lineLen, encoding);

        await onLineAsync(new FileLineReadResult
        {
            LineNumber = lineNumber,
            ByteOffset = lineStartOffset,
            ByteLength = effectiveLen,
            ConsumedByteLength = (int)(nextByteOffset - lineStartOffset),
            Line = lineString
        });
    }

    private static (int EffectiveLen, string LineString) DecodeLine(byte[] lineBuffer, int lineLen, Encoding encoding)
    {
        var effectiveLen = lineLen;
        if (effectiveLen > 0 && lineBuffer[effectiveLen - 1] == (byte)'\r')
            effectiveLen--;

        var lineString = effectiveLen == 0
            ? string.Empty
            : encoding.GetString(new ReadOnlySpan<byte>(lineBuffer, 0, effectiveLen));

        return (effectiveLen, lineString);
    }

    private async Task<string> ReadFirstMatchingLineAsync(
        Stream stream,
        Encoding encoding,
        string recordPrefix,
        CancellationToken cancellationToken)
    {
        await foreach (var line in ReadLinesAsync(stream, encoding, cancellationToken))
        {
            if (line.StartsWith(recordPrefix, StringComparison.OrdinalIgnoreCase))
                return line;
        }

        throw new FileIngestionRecordNotFoundFromStartException(_localizer.Get("FileIngestion.RecordNotFoundFromStart",
            recordPrefix));
    }

    private async Task<string> ReadLastMatchingLineAsync(
        Stream stream,
        Encoding encoding,
        string recordPrefix,
        CancellationToken cancellationToken)
    {
        if (!stream.CanSeek)
        {
            string? lastMatchingLine = null;

            await foreach (var line in ReadLinesAsync(stream, encoding, cancellationToken))
            {
                if (line.StartsWith(recordPrefix, StringComparison.OrdinalIgnoreCase))
                    lastMatchingLine = line;
            }

            return lastMatchingLine
                   ?? throw new FileIngestionRecordNotFoundFromEndException(
                       _localizer.Get("FileIngestion.RecordNotFoundFromEnd", recordPrefix));
        }

        const int blockSize = 64 * 1024; 
        var position = stream.Length;
        var buffer = ArrayPool<byte>.Shared.Rent(blockSize);
        var carry = ArrayPool<byte>.Shared.Rent(blockSize);
        var carryLen = 0;
        try
        {
            while (position > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var readSize = (int)Math.Min(blockSize, position);
                position -= readSize;
                stream.Seek(position, SeekOrigin.Begin);

                var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, readSize), cancellationToken);
                if (bytesRead == 0)
                    break;
                
                var combinedLen = bytesRead + carryLen;
                var combined = ArrayPool<byte>.Shared.Rent(combinedLen);
                try
                {
                    Buffer.BlockCopy(buffer, 0, combined, 0, bytesRead);
                    if (carryLen > 0)
                        Buffer.BlockCopy(carry, 0, combined, bytesRead, carryLen);
                    
                    var endIdx = combinedLen;
                    var firstNl = -1;
                    for (var i = combinedLen - 1; i >= 0; i--)
                    {
                        if (combined[i] != (byte)'\n') continue;
                        var lineStart = i + 1;
                        var lineLen = endIdx - lineStart;

                        if (lineLen > 0 && combined[lineStart + lineLen - 1] == (byte)'\r') lineLen--;
                        if (lineLen > 0)
                        {
                            var line = encoding.GetString(combined, lineStart, lineLen);
                            if (line.StartsWith(recordPrefix, StringComparison.OrdinalIgnoreCase))
                                return line;
                        }

                        endIdx = i;
                        firstNl = i;
                    }
                    
                    var carryNeed = firstNl < 0 ? combinedLen : firstNl;
                    if (carryNeed > carry.Length)
                    {
                        ArrayPool<byte>.Shared.Return(carry);
                        carry = ArrayPool<byte>.Shared.Rent(carryNeed);
                    }

                    Buffer.BlockCopy(combined, 0, carry, 0, carryNeed);
                    carryLen = carryNeed;
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(combined);
                }
            }

            if (carryLen > 0)
            {
                var tailLen = carryLen;
                if (tailLen > 0 && carry[tailLen - 1] == (byte)'\n') tailLen--;
                if (tailLen > 0 && carry[tailLen - 1] == (byte)'\r') tailLen--;
                if (tailLen > 0)
                {
                    var tail = encoding.GetString(carry, 0, tailLen);
                    if (tail.StartsWith(recordPrefix, StringComparison.OrdinalIgnoreCase))
                        return tail;
                }
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
            ArrayPool<byte>.Shared.Return(carry);
        }

        throw new FileIngestionRecordNotFoundFromEndException(_localizer.Get("FileIngestion.RecordNotFoundFromEnd",
            recordPrefix));
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        return ex.InnerException switch
        {
            PostgresException postgresEx when postgresEx.SqlState == PostgresErrorCodes.UniqueViolation => true,
            SqlException sqlEx when sqlEx.Number is 2601 or 2627 => true,
            _ => false
        };
    }

    private static IEnumerable<T[]> Batch<T>(T[] source, int batchSize)
    {
        for (var index = 0; index < source.Length; index += batchSize)
        {
            var length = Math.Min(batchSize, source.Length - index);
            var batch = new T[length];
            Array.Copy(source, index, batch, 0, length);
            yield return batch;
        }
    }

    private static async IAsyncEnumerable<string> ReadLinesAsync(
        Stream stream,
        Encoding encoding,
        [System.Runtime.CompilerServices.EnumeratorCancellation]
        CancellationToken cancellationToken)
    {
        const int ioBufferSize = 128 * 1024;
        const int initialLineBufferSize = 4 * 1024;

        var preamble = encoding.GetPreamble();
        if (preamble.Length > 0 && stream.CanSeek && stream.Position == 0 && stream.Length >= preamble.Length)
        {
            var preambleBuffer = new byte[preamble.Length];
            var preambleRead = await stream.ReadAsync(preambleBuffer.AsMemory(0, preamble.Length), cancellationToken);

            if (!(preambleRead == preamble.Length && preambleBuffer.AsSpan().SequenceEqual(preamble)))
                stream.Seek(0, SeekOrigin.Begin);
        }

        var buffer = ArrayPool<byte>.Shared.Rent(ioBufferSize);
        var lineBuffer = ArrayPool<byte>.Shared.Rent(initialLineBufferSize);
        var lineLen = 0;

        try
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
                if (bytesRead == 0)
                    break;

                var pos = 0;
                while (pos < bytesRead)
                {
                    var remaining = bytesRead - pos;
                    var nlAbs = Array.IndexOf(buffer, (byte)'\n', pos, remaining);

                    if (nlAbs < 0)
                    {
                        EnsureLineBufferCapacity(ref lineBuffer, lineLen, remaining);
                        Buffer.BlockCopy(buffer, pos, lineBuffer, lineLen, remaining);
                        lineLen += remaining;
                        break;
                    }

                    var lineSegmentLen = nlAbs - pos;
                    if (lineSegmentLen > 0)
                    {
                        EnsureLineBufferCapacity(ref lineBuffer, lineLen, lineSegmentLen);
                        Buffer.BlockCopy(buffer, pos, lineBuffer, lineLen, lineSegmentLen);
                        lineLen += lineSegmentLen;
                    }

                    yield return DecodeBufferedLine(lineBuffer, lineLen, encoding);
                    lineLen = 0;
                    pos = nlAbs + 1;
                }
            }

            if (lineLen > 0)
                yield return DecodeBufferedLine(lineBuffer, lineLen, encoding);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
            ArrayPool<byte>.Shared.Return(lineBuffer);
        }
    }

    private static string DecodeBufferedLine(byte[] lineBuffer, int lineLen, Encoding encoding)
    {
        var effective = lineLen;
        if (effective > 0 && lineBuffer[effective - 1] == (byte)'\r')
            effective--;

        return effective == 0
            ? string.Empty
            : encoding.GetString(new ReadOnlySpan<byte>(lineBuffer, 0, effective));
    }

    private FileIngestionResponse CreateErrorResponse(
        string fileName,
        List<IngestionErrorDetail> errors)
    {
        var firstMessage = errors.FirstOrDefault()?.Message;
        return new FileIngestionResponse
        {
            FileId = Guid.Empty,
            FileKey = string.Empty,
            FileName = fileName,
            Status = FileStatus.Failed,
            Message = string.IsNullOrWhiteSpace(firstMessage)
                ? _localizer.Get("FileIngestion.IngestionFailedDefault")
                : _localizer.Get("FileIngestion.IngestionFailedWithMessage", firstMessage),
            TotalCount = 0,
            SuccessCount = 0,
            ErrorCount = errors.Count,
            Errors = errors
        };
    }

    private static FileIngestionResponse ToResponse(IngestionFileEntity file)
    {
        return new FileIngestionResponse
        {
            FileId = file.Id,
            FileKey = file.FileKey,
            FileName = file.FileName,
            Status = file.Status,
            Message = file.Message,
            TotalCount = file.ProcessedLineCount,
            SuccessCount = file.SuccessfulLineCount,
            ErrorCount = file.FailedLineCount
        };
    }

    private sealed class BoundaryRecord
    {
        public string RawLine { get; init; } = string.Empty;
        public ParsedFileLine ParsedLine { get; init; } = new();
        public object TypedModel { get; init; } = default!;
    }

    private sealed class ProcessingProgress
    {
        public long TotalCount { get; set; }
        public long SuccessCount { get; set; }
        public long ErrorCount { get; set; }
        public long LastProcessedLineNumber { get; set; }
        public long LastProcessedByteOffset { get; set; }
        public int BatchSequence { get; set; }
    }

    private sealed class TargetWriter
    {
        private Stream? _stream;
        private readonly ILogger _logger;
        private readonly IStringLocalizer _localizer;

        public TargetWriter(Stream? stream, ILogger logger, IStringLocalizer localizer, string? errorMessage = null)
        {
            _stream = stream;
            _logger = logger;
            _localizer = localizer;
            ErrorMessage = errorMessage;
        }

        public string? ErrorMessage { get; private set; }

        public string? ErrorStep { get; private set; }

        public async Task WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
        {
            if (_stream is null || !string.IsNullOrWhiteSpace(ErrorMessage))
                return;

            try
            {
                await _stream.WriteAsync(buffer, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, _localizer.Get("FileIngestion.TargetArchiveWriteFailed"), ex.Message);
                ErrorMessage = BuildDetailedErrorMessage(ex);
                ErrorStep = "ARCHIVE_STREAM_WRITE";
                try
                {
                    await _stream.DisposeAsync();
                }
                catch
                {
                    /* best-effort cleanup */
                }

                _stream = null;
            }
        }

        public async Task FinalizeAsync(CancellationToken cancellationToken)
        {
            if (_stream is null)
                return;

            try
            {
                await _stream.FlushAsync(cancellationToken);
                await _stream.DisposeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, _localizer.Get("FileIngestion.TargetArchiveFinalizeFailed"), ex.Message);
                ErrorMessage = BuildDetailedErrorMessage(ex);
                ErrorStep = "ARCHIVE_STREAM_FINALIZE";
            }
            finally
            {
                _stream = null;
            }
        }

        private static string BuildDetailedErrorMessage(Exception ex)
        {
            var message = $"[{ex.GetType().Name}] {ex.Message}";
            if (ex.InnerException != null)
                message += $" -> [{ex.InnerException.GetType().Name}] {ex.InnerException.Message}";
            return message;
        }
    }
}
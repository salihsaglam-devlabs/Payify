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
using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using LinkPara.Card.Application.Commons.Extensions;
using LinkPara.Card.Application.Commons.Helpers;
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
    private readonly CardDbContext _dbContext;
    private readonly IAuditStampService _auditStampService;
    private readonly IFileTransferClientResolver _fileTransferClientResolver;
    private readonly IFixedWidthRecordParser _fixedWidthRecordParser;
    private readonly IParsedRecordModelMapper _parsedRecordModelMapper;
    private readonly IIngestionDetailEntityMapper _detailEntityMapper;
    private readonly FileIngestionOptions _options;
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
        IOptions<FileIngestionOptions> options,
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
        _options = options.Value;
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
                return new List<FileIngestionResponse> { CreateErrorResponse(request.FilePath ?? "unknown", globalErrors) };
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
                return new List<FileIngestionResponse> { CreateErrorResponse(error.FileName ?? request.FilePath ?? "unknown", globalErrors) };
            }

            if (files.Count == 0)
            {
                var error = new IngestionErrorDetail
                {
                    Code = "FILE_NOT_FOUND",
                    Message = _localizer.Get("FileIngestion.NoFileMatchedProfile", profileKey),
                    Detail = $"Profile: {profileKey}, FileSourceType: {request.FileSourceType}, FilePath: {request.FilePath}",
                    Step = "FILE_RESOLUTION",
                    FileName = request.FilePath ?? "unknown",
                    Severity = "Error"
                };
                globalErrors.Add(error);
                return new List<FileIngestionResponse> { CreateErrorResponse(request.FilePath ?? "unknown", globalErrors) };
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
                                var scopedOrchestrator = (FileIngestionOrchestrator)scope.ServiceProvider.GetRequiredService<IFileIngestionService>();

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

            return new List<FileIngestionResponse> { CreateErrorResponse(request?.FilePath ?? "unknown", new List<IngestionErrorDetail> { error }) };
        }
        catch (Exception ex)
        {
            var error = _ingestionErrorMapper.MapException(ex, "INGESTION", fileName: request?.FilePath);
            return new List<FileIngestionResponse> { CreateErrorResponse(request?.FilePath ?? "unknown", new List<IngestionErrorDetail> { error }) };
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
            error.Detail = _localizer.Get("FileIngestion.Detail.BoundaryReadFailed", ExceptionDetailHelper.BuildDetailMessage(ex));
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
                await UpdateFileMessageAsync(existing.Id, _localizer.Get("FileIngestion.DuplicateFileReceived"), cancellationToken);
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
            await _dbContext.IngestionFiles.AddAsync(transactionFile, cancellationToken);
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
                    await PersistBatchAsync(transactionFileId, batch, progress, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            var error = _ingestionErrorMapper.MapException(ex, "FILE_PROCESSING", fileName: file.Name);
            error.Detail = _localizer.Get("FileIngestion.Detail.FileProcessingError", progress.LastProcessedLineNumber, ExceptionDetailHelper.BuildDetailMessage(ex));
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
                file = await RetryArchiveOnlyAsync(transactionFileId, sourceFile, sourceTransferClient, errors, cancellationToken);
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
                await PersistBatchAsync(file.Id, batch, progress, cancellationToken);
        }
        catch (Exception ex)
        {
            var error = _ingestionErrorMapper.MapException(ex, "RECOVERY_MISSING_ROWS", fileName: sourceFile.Name);
            error.Detail = _localizer.Get("FileIngestion.Detail.RecoveryMissingRowsProcessingError", progress.LastProcessedLineNumber, ExceptionDetailHelper.BuildDetailMessage(ex));
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

                        var rawLine = await sourceTransferClient.ReadRangeAsync(
                            sourceFile,
                            row.ByteOffset,
                            row.ByteLength,
                            ResolveEncoding(profile.DefaultEncoding),
                            cancellationToken);

                        var parsed = _fixedWidthRecordParser.Parse(rawLine, parsingRule);
                        parsed.ParsedDataModel = _parsedRecordModelMapper.Create(profileKey, parsed);

                        row.RawContent = rawLine;
                        row.LineType = parsed.RecordType;
                        row.ParsedContent = JsonSerializer.Serialize(parsed.ParsedDataModel);
                        row.Status = FileRowStatus.Success;
                        row.RetryCount += 1;
                        row.Message = _localizer.Get("FileIngestion.ReprocessedSuccessfully");
                        ApplyCorrelation(row, profileKey, GetFileTypeFromProfileKey(profileKey), parsed.ParsedDataModel);
                        _detailEntityMapper.AttachTypedDetail(row, profileKey, parsed.ParsedDataModel);
                    }
                    catch (Exception ex)
                    {
                        var exceptionDetail = ExceptionDetailHelper.BuildDetailMessage(ex, 2000);
                        var error = _ingestionErrorMapper.MapException(ex, "ROW_RETRY", fileName: sourceFile.Name);
                        error.LineNumber = row.LineNumber;
                        error.Detail = _localizer.Get("FileIngestion.Detail.RowRetryFailed", row.LineNumber, exceptionDetail);
                        errors.Add(error);
                        
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

            var exhaustedRows = await _dbContext.IngestionFileLines
                .AsNoTracking()
                .Where(x => x.FileId == transactionFileId && x.Status == FileRowStatus.Failed)
                .Where(x => x.RetryCount >= GetFailedRowMaxRetryCount())
                .OrderBy(x => x.LineNumber)
                .Select(x => new { x.LineNumber, x.Message })
                .ToListAsync(cancellationToken);

            foreach (var exhaustedRow in exhaustedRows)
            {
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

            await FinalizeFileStateAsync(transactionFileId, null, cancellationToken);
        }
        catch (Exception ex)
        {
            var error = _ingestionErrorMapper.MapException(ex, "RECOVERY_FAILED_ROWS", fileName: sourceFile.Name);
            error.Detail = _localizer.Get("FileIngestion.Detail.FailedRowBatchRetryFailed");
            errors.Add(error);
            throw;
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
                            _localizer.Get("FileIngestion.ArchiveStatusUpdateRowMismatch", affectedRows, transactionFileId));
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
        CancellationToken cancellationToken)
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            await PersistIngestionFileLineBatchAsync(batch, cancellationToken);

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
        var details = new List<AuditEntity>();
        foreach (var row in rows)
        {
            var detail = ExtractTypedDetail(row);
            if (detail is null) continue;
            ((IIngestionTypedDetail)detail).FileLineId = row.Id;
            details.Add(detail);
        }

        if (details.Count == 0)
            return;

        _auditStampService.StampForCreate(details);
        _dbContext.AddRange(details);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    
    private void StampTypedDetails(IReadOnlyList<IngestionFileLineEntity> rows)
    {
        var details = rows
            .Select(ExtractTypedDetail)
            .Where(d => d is not null)
            .Cast<AuditEntity>()
            .ToList();

        if (details.Count > 0)
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
                ?? throw new FileIngestionEntityTypeNotMappedException(_localizer.Get("FileIngestion.EntityTypeNotMapped"));

            var storeObject = StoreObjectIdentifier.Table(entityType.GetTableName()!, entityType.GetSchema());
            var properties = GetBulkProperties(entityType, storeObject);

            var dataTable = new DataTable();

            foreach (var property in properties)
            {
                var columnType = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;
                if (columnType.IsEnum)
                    columnType = typeof(string);

                dataTable.Columns.Add(property.GetColumnName(storeObject)!, columnType);
            }

            _auditStampService.StampForCreate(rows.Cast<AuditEntity>());
            foreach (var row in rows)
            {
                var values = properties
                    .Select(property => GetDatabaseValue(property, row) ?? DBNull.Value)
                    .ToArray();

                dataTable.Rows.Add(values);
            }

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
                BatchSize = rows.Count,
                BulkCopyTimeout = 600
            };

            foreach (var property in properties)
            {
                var columnName = property.GetColumnName(storeObject)!;
                bulkCopy.ColumnMappings.Add(columnName, columnName);
            }

            await bulkCopy.WriteToServerAsync(dataTable, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new FileIngestionSqlBulkInsertFailedException(
                _localizer.Get("FileIngestion.SqlBulkInsertFailed", ex.Message),
                ex);
        }
    }

    private async Task BulkInsertPostgreSqlAsync(
        IReadOnlyList<IngestionFileLineEntity> rows,
        CancellationToken cancellationToken)
    {
        try
        {
            var entityType = _dbContext.Model.FindEntityType(typeof(IngestionFileLineEntity))
                ?? throw new FileIngestionEntityTypeNotMappedException(_localizer.Get("FileIngestion.EntityTypeNotMapped"));

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
                await importer.StartRowAsync(cancellationToken);

                foreach (var property in properties)
                {
                    var value = GetDatabaseValue(property, row);
                    if (value is null)
                    {
                        await importer.WriteNullAsync(cancellationToken);
                        continue;
                    }

                    await WriteNpgsqlValueAsync(importer, value, property.ClrType, cancellationToken);
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

        return propertyNames
            .Select(name => entityType.FindProperty(name)
                ?? throw new FileIngestionPropertyNotMappedException(
                    _localizer.Get("FileIngestion.PropertyNotMapped", name, entityType.Name)))            .Where(property => property.GetColumnName(storeObject) is not null)
            .ToArray();
    }

    private static object? GetDatabaseValue(IProperty property, object entity)
    {
        var value = property.PropertyInfo?.GetValue(entity);
        if (value is null)
            return null;

        var clrType = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;
        return clrType.IsEnum ? value.ToString() : value;
    }

    private Task WriteNpgsqlValueAsync(
        NpgsqlBinaryImporter importer,
        object value,
        Type clrType,
        CancellationToken cancellationToken)
    {
        var effectiveType = Nullable.GetUnderlyingType(clrType) ?? clrType;

        if (effectiveType.IsEnum)
            return importer.WriteAsync(value.ToString(), NpgsqlDbType.Text, cancellationToken);

        if (effectiveType == typeof(Guid))
            return importer.WriteAsync((Guid)value, NpgsqlDbType.Uuid, cancellationToken);
        if (effectiveType == typeof(string))
            return importer.WriteAsync((string)value, NpgsqlDbType.Text, cancellationToken);
        if (effectiveType == typeof(long))
            return importer.WriteAsync((long)value, NpgsqlDbType.Bigint, cancellationToken);
        if (effectiveType == typeof(int))
            return importer.WriteAsync((int)value, NpgsqlDbType.Integer, cancellationToken);
        if (effectiveType == typeof(short))
            return importer.WriteAsync((short)value, NpgsqlDbType.Smallint, cancellationToken);
        if (effectiveType == typeof(bool))
            return importer.WriteAsync((bool)value, NpgsqlDbType.Boolean, cancellationToken);
        if (effectiveType == typeof(DateTime))
            return importer.WriteAsync((DateTime)value, NpgsqlDbType.Timestamp, cancellationToken);

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
                ParsedContent = JsonSerializer.Serialize(parsed.ParsedDataModel),
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
                Message = ExceptionDetailHelper.BuildDetailMessage(ex, 2000),
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

        var correlationValues = fileType switch
        {
            FileType.Card => new[]
            {
                GetStringValue(parsedDataModel, "Rrn"),
                GetStringValue(parsedDataModel, "CardNo"),
                GetStringValue(parsedDataModel, "ProvisionCode"),
                GetStringValue(parsedDataModel, "Arn"),
                GetNormalizedValue(parsedDataModel, "Mcc"),
                GetNormalizedValue(parsedDataModel, "CardHolderBillingAmount"),
                GetNormalizedValue(parsedDataModel, "CardHolderBillingCurrency")
            },
            FileType.Clearing => new[]
            {
                GetStringValue(parsedDataModel, "Rrn"),
                GetStringValue(parsedDataModel, "CardNo"),
                GetStringValue(parsedDataModel, "ProvisionCode"),
                GetStringValue(parsedDataModel, "Arn"),
                GetNormalizedValue(parsedDataModel, "MccCode"),
                GetNormalizedValue(parsedDataModel, "SourceAmount"),
                GetNormalizedValue(parsedDataModel, "SourceCurrency")
            },
            _ => Array.Empty<string>()
        };

        row.CorrelationValue = string.Join(":", correlationValues);
        row.ReconciliationStatus = correlationValues.Length == 0 || correlationValues.Any(string.IsNullOrWhiteSpace)
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
                // Requeue başarısızlığı dosyanın Success marklanmasını geri almaz;
                // ancak monitoring/alert için loglanmalıdır. Bir sonraki clearing
                // ingestion ya da scheduled re-evaluation bu satırları yakalar.
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

        List<IngestionFileLineEntity> rowsToUpdate;
        rowsToUpdate = await LoadDuplicateRowsAsync(transactionFileId, cancellationToken);

        if (file.FileType == FileType.Card)
        {
            ApplyCardDuplicateOutcomes(rowsToUpdate);
        }
        else
        {
            ApplyClearingDuplicateOutcomes(rowsToUpdate);
        }

        if (rowsToUpdate.Count > 0)
        {
            _auditStampService.StampForUpdate(rowsToUpdate.Cast<AuditEntity>());
            await _dbContext.SaveChangesAsync(cancellationToken);
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

        if (file.FileType == FileType.Clearing)
        {
            var clearingLines = await _dbContext.IngestionFileLines
                .AsNoTracking()
                .Where(x => x.FileId == transactionFileId
                            && x.LineType == "D"
                            && x.Status == FileRowStatus.Success
                            && x.CorrelationKey != null
                            && x.CorrelationValue != null)
                .Select(x => new { x.Id, x.CorrelationKey, x.CorrelationValue })
                .ToListAsync(cancellationToken);

            foreach (var clearingLine in clearingLines)
            {
                await _dbContext.IngestionFileLines
                    .Where(x => x.CorrelationKey == clearingLine.CorrelationKey
                                && x.CorrelationValue == clearingLine.CorrelationValue
                                && x.LineType == "D"
                                && x.IngestionFile.FileType == FileType.Card)
                    .ExecuteUpdateAsync(update => update
                            .SetProperty(x => x.MatchedClearingLineId, clearingLine.Id)
                            .SetProperty(x => x.UpdateDate, auditStamp.Timestamp)
                            .SetProperty(x => x.LastModifiedBy, auditStamp.UserId),
                        cancellationToken);
            }
        }
        else if (file.FileType == FileType.Card)
        {
            var cardLines = await _dbContext.IngestionFileLines
                .AsNoTracking()
                .Where(x => x.FileId == transactionFileId
                            && x.LineType == "D"
                            && x.Status == FileRowStatus.Success
                            && x.CorrelationKey != null
                            && x.CorrelationValue != null)
                .Select(x => new { x.Id, x.CorrelationKey, x.CorrelationValue })
                .ToListAsync(cancellationToken);

            foreach (var cardLine in cardLines)
            {
                var latestClearingLineId = await _dbContext.IngestionFileLines
                    .AsNoTracking()
                    .Where(x => x.CorrelationKey == cardLine.CorrelationKey
                                && x.CorrelationValue == cardLine.CorrelationValue
                                && x.LineType == "D"
                                && x.IngestionFile.FileType == FileType.Clearing)
                    .OrderByDescending(x => x.CreateDate)
                    .Select(x => (Guid?)x.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (latestClearingLineId.HasValue)
                {
                    await _dbContext.IngestionFileLines
                        .Where(x => x.Id == cardLine.Id)
                        .ExecuteUpdateAsync(update => update
                                .SetProperty(x => x.MatchedClearingLineId, latestClearingLineId.Value)
                                .SetProperty(x => x.UpdateDate, auditStamp.Timestamp)
                                .SetProperty(x => x.LastModifiedBy, auditStamp.UserId),
                            cancellationToken);
                }
            }
        }

        _dbContext.ChangeTracker.Clear();
    }

    private async Task<List<IngestionFileLineEntity>> LoadDuplicateRowsAsync(
        Guid transactionFileId,
        CancellationToken cancellationToken)
    {
        var duplicateKeys = await _dbContext.IngestionFileLines
            .AsNoTracking()
            .Where(x => x.FileId == transactionFileId &&
                        x.LineType == "D" &&
                        x.Status == FileRowStatus.Success &&
                        x.DuplicateDetectionKey != null)
            .GroupBy(x => x.DuplicateDetectionKey!)
            .Where(x => x.Count() > 1)
            .Select(x => x.Key)
            .ToListAsync(cancellationToken);

        if (duplicateKeys.Count == 0)
        {
            return new List<IngestionFileLineEntity>();
        }

        var rows = new List<IngestionFileLineEntity>();
        foreach (var batch in Batch(duplicateKeys.ToArray(), QueryBatchSize))
        {
            var batchRows = await _dbContext.IngestionFileLines
                .Where(x => x.FileId == transactionFileId &&
                            x.LineType == "D" &&
                            x.Status == FileRowStatus.Success &&
                            batch.Contains(x.DuplicateDetectionKey!))
                .OrderBy(x => x.LineNumber)
                .ThenBy(x => x.Id)
                .ToListAsync(cancellationToken);

            rows.AddRange(batchRows);
        }

        return rows;
    }

    private static void ApplyCardDuplicateOutcomes(List<IngestionFileLineEntity> detailRows)
    {
        var duplicateGroups = detailRows
            .Where(x => !string.IsNullOrWhiteSpace(x.DuplicateDetectionKey))
            .GroupBy(x => x.DuplicateDetectionKey, StringComparer.Ordinal)
            .Where(x => x.Count() > 1);

        foreach (var group in duplicateGroups)
        {
            var rows = group.ToList();
            var primary = rows[0];
            var primarySignature = BuildCardDuplicateSignature(primary.ParsedContent);
            var allEquivalent = rows.All(x => CardDuplicateSignatureEquals(primarySignature, BuildCardDuplicateSignature(x.ParsedContent)));

            if (allEquivalent)
            {
                MarkEquivalentDuplicateGroup(
                    rows,
                    primary);
                continue;
            }

            MarkConflictingDuplicateGroup(
                rows);
        }
    }

    private static void ApplyClearingDuplicateOutcomes(List<IngestionFileLineEntity> detailRows)
    {
        var duplicateGroups = detailRows
            .Where(x => !string.IsNullOrWhiteSpace(x.DuplicateDetectionKey))
            .GroupBy(x => x.DuplicateDetectionKey, StringComparer.Ordinal)
            .Where(x => x.Count() > 1);

        foreach (var group in duplicateGroups)
        {
            var rows = group.ToList();
            var primary = rows[0];
            var primaryPayload = primary.ParsedContent;
            var allEquivalent = rows.All(x => string.Equals(x.ParsedContent, primaryPayload, StringComparison.Ordinal));

            if (allEquivalent)
            {
                MarkEquivalentDuplicateGroup(
                    rows,
                    primary);
                continue;
            }

            MarkConflictingDuplicateGroup(
                rows);
        }
    }

    private static void MarkEquivalentDuplicateGroup(
        List<IngestionFileLineEntity> rows,
        IngestionFileLineEntity primary)
    {
        var duplicateGroupId = Guid.NewGuid();
        primary.DuplicateStatus = DuplicateStatus.Primary.ToString();
        primary.DuplicateGroupId = duplicateGroupId;
        primary.ReconciliationStatus ??= ReconciliationStatus.Ready;

        foreach (var secondary in rows.Skip(1))
        {
            secondary.DuplicateStatus = DuplicateStatus.Secondary.ToString();
            secondary.DuplicateGroupId = duplicateGroupId;
            secondary.ReconciliationStatus = ReconciliationStatus.Failed;
        }
    }

    private static void MarkConflictingDuplicateGroup(
        IEnumerable<IngestionFileLineEntity> rows)
    {
        var duplicateGroupId = Guid.NewGuid();
        foreach (var row in rows)
        {
            row.DuplicateStatus = DuplicateStatus.Conflict.ToString();
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
        if (!element.TryGetProperty(propertyName, out var value) || value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
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
        if (!element.TryGetProperty(propertyName, out var value) || value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
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
            await using var stream = await transferClient.OpenReadAsync(file, cancellationToken);
            using var reader = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks: true, leaveOpen: true);

            string? headerLine = null;
            string? footerLine = null;

            while (await reader.ReadLineAsync(cancellationToken) is { } line)
            {
                if (headerLine is null && line.StartsWith(parsingRule.HeaderPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    headerLine = line;
                }

                if (line.StartsWith(parsingRule.FooterPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    footerLine = line;
                }
            }

            if (headerLine is null)
            {
                throw new FileIngestionHeaderNotResolvedException( _localizer.Get("FileIngestion.HeaderNotResolved", file.Name));
            }

            if (footerLine is null)
            {
                throw new FileIngestionFooterNotResolvedException( _localizer.Get("FileIngestion.FooterNotResolved", file.Name));
            }

            return (
                ParseBoundaryRecord(profileKey, parsingRule, headerLine, parsingRule.HeaderPrefix),
                ParseBoundaryRecord(profileKey, parsingRule, footerLine, parsingRule.FooterPrefix));
        }
        catch (Exception ex)
        {
            throw new FileIngestionBoundaryRecordReadFailedException( _localizer.Get("FileIngestion.BoundaryRecordReadFailed", file.Name, ex.Message), ex);
        }
    }

    private BoundaryRecord ParseBoundaryRecord(
        string profileKey,
        ParsingOptions parsingRule,
        string line,
        string expectedRecordType)
    {
        if (string.IsNullOrWhiteSpace(line))
            throw new FileIngestionBoundaryRecordEmptyException( _localizer.Get("FileIngestion.BoundaryRecordEmpty", expectedRecordType));

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
            throw new FileIngestionPropertyEmptyException( _localizer.Get("FileIngestion.PropertyEmpty", propertyName, instance.GetType().Name));

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
                throw new FileIngestionFilePatternMismatchException(_localizer.Get("FileIngestion.FilePatternMismatch", explicitFileName));

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
            throw new FileIngestionProfileNotConfiguredException(_localizer.Get("FileIngestion.ProfileNotConfigured", profileKey));

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
        var allowedExtensions = new HashSet<string>(
            (profile.FileExtensions ?? Enumerable.Empty<string>())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!.Trim())
            .Select(x => x.StartsWith('.') ? x : "." + x),
            StringComparer.OrdinalIgnoreCase);

        var pattern = profile.Pattern ?? string.Empty;
        if (pattern.EndsWith("$", StringComparison.Ordinal))
            pattern = pattern[..^1];

        if (allowedExtensions.Count > 0)
        {
            var extensionPattern = string.Join("|", allowedExtensions.Select(Regex.Escape));
            pattern = $"{pattern}(?:{extensionPattern})$";
        }
        else
        {
            pattern = $"{pattern}$";
        }

        var regex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
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

    private static string GetStringValue(object model, string propertyName)
    {
        var property = model.GetType().GetProperty(propertyName);
        if (property is null)
            return string.Empty;

        return Convert.ToString(property.GetValue(model), CultureInfo.InvariantCulture)?.Trim() ?? string.Empty;
    }

    private static string GetNormalizedValue(object model, string propertyName)
    {
        var property = model.GetType().GetProperty(propertyName);
        if (property is null)
            return string.Empty;

        var value = property.GetValue(model);

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

        var property = model.GetType().GetProperty(propertyName);
        if (property is null)
            return false;

        var rawValue = property.GetValue(model);
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
        await using var lineBuffer = new MemoryStream();

        var lineNumber = startingLineNumber;
        var lineStartOffset = stream.CanSeek ? stream.Position : startingByteOffset;
        var currentOffset = lineStartOffset;

        try
        {
            while (true)
            {
                var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
                if (bytesRead == 0)
                    break;

                if (onBytesAsync is not null)
                    await onBytesAsync(buffer.AsMemory(0, bytesRead), cancellationToken);

                for (var index = 0; index < bytesRead; index++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var currentByte = buffer[index];

                    if (currentByte == (byte)'\n')
                    {
                        await EmitLineAsync(
                            lineBuffer,
                            encoding,
                            onLineAsync,
                            ++lineNumber,
                            lineStartOffset,
                            currentOffset + 1);

                        lineStartOffset = currentOffset + 1;
                    }
                    else
                    {
                        lineBuffer.WriteByte(currentByte);
                    }

                    currentOffset++;
                }
            }

            if (lineBuffer.Length > 0)
                await EmitLineAsync(lineBuffer, encoding, onLineAsync, ++lineNumber, lineStartOffset, currentOffset);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private static async Task EmitLineAsync(
        MemoryStream lineBuffer,
        Encoding encoding,
        Func<FileLineReadResult, Task> onLineAsync,
        long lineNumber,
        long lineStartOffset,
        long nextByteOffset)
    {
        var lineBytes = lineBuffer.ToArray();
        var lineLength = lineBytes.Length;

        if (lineLength > 0 && lineBytes[^1] == (byte)'\r')
            lineLength--;

        await onLineAsync(new FileLineReadResult
        {
            LineNumber = lineNumber,
            ByteOffset = lineStartOffset,
            ByteLength = lineLength,
            ConsumedByteLength = (int)(nextByteOffset - lineStartOffset),
            Line = encoding.GetString(lineBytes, 0, lineLength)
        });

        lineBuffer.SetLength(0);
    }

    private async Task<string> ReadFirstMatchingLineAsync(
        Stream stream,
        Encoding encoding,
        string recordPrefix,
        CancellationToken cancellationToken)
    {
        await foreach (var line in ReadLinesAsync(stream, encoding, cancellationToken))
        {
            if (line.StartsWith(recordPrefix, StringComparison.Ordinal))
                return line;
        }

        throw new FileIngestionRecordNotFoundFromStartException(_localizer.Get("FileIngestion.RecordNotFoundFromStart", recordPrefix));
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
                if (line.StartsWith(recordPrefix, StringComparison.Ordinal))
                    lastMatchingLine = line;
            }

            return lastMatchingLine
                ?? throw new FileIngestionRecordNotFoundFromEndException(_localizer.Get("FileIngestion.RecordNotFoundFromEnd", recordPrefix));
        }

        const int blockSize = 8192;
        var position = stream.Length;
        var carry = Array.Empty<byte>();

        while (position > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var readSize = (int)Math.Min(blockSize, position);
            position -= readSize;
            stream.Seek(position, SeekOrigin.Begin);

            var buffer = new byte[readSize];
            var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, readSize), cancellationToken);
            if (bytesRead == 0)
                break;

            var combined = new byte[bytesRead + carry.Length];
            Buffer.BlockCopy(buffer, 0, combined, 0, bytesRead);

            if (carry.Length > 0)
                Buffer.BlockCopy(carry, 0, combined, bytesRead, carry.Length);

            var combinedText = encoding.GetString(combined);
            var lines = combinedText.Split('\n');
            carry = encoding.GetBytes(lines[0]);

            for (var index = lines.Length - 1; index >= 1; index--)
            {
                var line = lines[index].TrimEnd('\r');
                if (line.StartsWith(recordPrefix, StringComparison.Ordinal))
                    return line;
            }
        }

        var tail = encoding.GetString(carry).TrimEnd('\r', '\n');
        if (tail.StartsWith(recordPrefix, StringComparison.Ordinal))
            return tail;

        throw new FileIngestionRecordNotFoundFromEndException(_localizer.Get("FileIngestion.RecordNotFoundFromEnd", recordPrefix));
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

        var preamble = encoding.GetPreamble();
        if (preamble.Length > 0 && stream.CanSeek && stream.Position == 0 && stream.Length >= preamble.Length)
        {
            var preambleBuffer = new byte[preamble.Length];
            var preambleRead = await stream.ReadAsync(preambleBuffer.AsMemory(0, preamble.Length), cancellationToken);

            if (!(preambleRead == preamble.Length && preambleBuffer.AsSpan().SequenceEqual(preamble)))
                stream.Seek(0, SeekOrigin.Begin);
        }

        var buffer = ArrayPool<byte>.Shared.Rent(ioBufferSize);
        await using var lineBuffer = new MemoryStream();

        try
        {
            while (true)
            {
                var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
                if (bytesRead == 0)
                    break;

                for (var index = 0; index < bytesRead; index++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var currentByte = buffer[index];

                    if (currentByte == (byte)'\n')
                    {
                        yield return DecodeBufferedLine(lineBuffer, encoding);
                        lineBuffer.SetLength(0);
                        continue;
                    }

                    lineBuffer.WriteByte(currentByte);
                }
            }

            if (lineBuffer.Length > 0)
                yield return DecodeBufferedLine(lineBuffer, encoding);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private static string DecodeBufferedLine(MemoryStream lineBuffer, Encoding encoding)
    {
        var lineBytes = lineBuffer.ToArray();
        var lineLength = lineBytes.Length;

        if (lineLength > 0 && lineBytes[^1] == (byte)'\r')
            lineLength--;

        return encoding.GetString(lineBytes, 0, lineLength);
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
                try { await _stream.DisposeAsync(); } catch { /* best-effort cleanup */ }
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

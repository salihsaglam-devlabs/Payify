using LinkPara.Card.Application.Commons.Exceptions;
using LinkPara.Card.Application.Commons.Interfaces.Archive;
using LinkPara.Card.Application.Commons.Models.Archive;
using LinkPara.Card.Infrastructure.Services.Audit;
using LinkPara.ContextProvider;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LinkPara.Card.Infrastructure.Services.Archive;

internal sealed class ArchiveService : IArchiveService
{
    private const string SystemUserId = "SYSTEM_ARCHIVE";

    private static readonly HashSet<string> SupportedBeforeDateStrategies =
        new(StringComparer.OrdinalIgnoreCase) { "None", "RetentionDays" };

    private readonly ArchiveAggregateReader _reader;
    private readonly ArchiveEligibilityEvaluator _evaluator;
    private readonly ArchiveOptions _options;
    private readonly IArchiveErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly IServiceProvider _serviceProvider;
    private readonly IAuditUserContextAccessor _auditUserContextAccessor;
    private readonly IContextProvider _contextProvider;

    public ArchiveService(
        ArchiveAggregateReader reader,
        ArchiveEligibilityEvaluator evaluator,
        IOptions<ArchiveOptions> options,
        IArchiveErrorMapper errorMapper,
        Func<LinkPara.Card.Application.Commons.Localization.LocalizerResource, IStringLocalizer> localizerFactory,
        IServiceProvider serviceProvider,
        IAuditUserContextAccessor auditUserContextAccessor,
        IContextProvider contextProvider)
    {
        _reader = reader;
        _evaluator = evaluator;
        _options = options.Value ?? throw new InvalidOperationException("ArchiveOptions is not configured.");
        _errorMapper = errorMapper;
        _localizer = localizerFactory(LinkPara.Card.Application.Commons.Localization.LocalizerResource.Messages);
        _serviceProvider = serviceProvider;
        _auditUserContextAccessor = auditUserContextAccessor;
        _contextProvider = contextProvider;
    }

    public async Task<ArchivePreviewResponse> PreviewAsync(
        ArchivePreviewRequest request,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<ArchiveErrorDetail>();
        try
        {
            var effectiveRequest = request ?? new ArchivePreviewRequest();
            var effectiveBeforeDate = ResolveBeforeDate(effectiveRequest.BeforeDate);
            var effectiveLimit = effectiveRequest.Limit is null or 0
                ? _options.Defaults.PreviewLimit.Value
                : Math.Clamp(effectiveRequest.Limit.Value, 1, 1000);
            var candidateIds = await _reader.ResolveCandidateFileIdsAsync(
                effectiveRequest.IngestionFileIds ?? Array.Empty<Guid>(),
                effectiveBeforeDate,
                effectiveLimit,
                cancellationToken);

            var response = new ArchivePreviewResponse();
            foreach (var candidateId in candidateIds)
            {
                try
                {
                    var snapshot = await _reader.GetSnapshotAsync(candidateId, cancellationToken);
                    var eligibility = _evaluator.Evaluate(snapshot, DateTime.UtcNow);
                    response.Candidates.Add(new ArchiveCandidateResult
                    {
                        IngestionFileId = candidateId,
                        IsEligible = eligibility.IsEligible,
                        FailureReasons = eligibility.FailureReasons,
                        Counts = snapshot?.Counts ?? new ArchiveAggregateCounts()
                    });
                }
                catch (Exception ex)
                {
                    errors.Add(_errorMapper.MapException(ex, "ARCHIVE_PREVIEW_CANDIDATE", ingestionFileId: candidateId));
                    response.Candidates.Add(new ArchiveCandidateResult
                    {
                        IngestionFileId = candidateId,
                        IsEligible = false,
                        FailureReasons = new List<string> { "PREVIEW_EVALUATION_FAILED" }
                    });
                }
            }

            response.Errors = errors;
            response.ErrorCount = errors.Count;
            return response;
        }
        catch (Exception ex)
        {
            errors.Add(_errorMapper.MapException(ex, "ARCHIVE_SERVICE_PREVIEW"));
            return new ArchivePreviewResponse
            {
                Message = BuildFailureMessage(_localizer.Get("Archive.PreviewFailed"), errors),
                Errors = errors,
                ErrorCount = errors.Count
            };
        }
    }

    public async Task<ArchiveRunResponse> RunAsync(
        ArchiveRunRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureAuditContext();
        var executor = _serviceProvider.GetRequiredService<ArchiveExecutor>();
        var errors = new List<ArchiveErrorDetail>();
        try
        {
            var effectiveRequest = request ?? new ArchiveRunRequest();
            var effectiveBeforeDate = ResolveBeforeDate(effectiveRequest.BeforeDate);
            var effectiveMaxFiles = effectiveRequest.MaxFiles is null or 0
                ? _options.Defaults.MaxRunCount.Value
                : Math.Clamp(effectiveRequest.MaxFiles.Value, 1, 1000);
            var continueOnError = effectiveRequest.ContinueOnError ?? _options.Defaults.ContinueOnError.Value;
            var candidateIds = await _reader.ResolveCandidateFileIdsAsync(
                effectiveRequest.IngestionFileIds ?? Array.Empty<Guid>(),
                effectiveBeforeDate,
                effectiveMaxFiles,
                cancellationToken);

            var response = new ArchiveRunResponse();
            Guid? batchId = null;

            try
            {
                batchId = await executor.CreateBatchAsync(effectiveRequest, cancellationToken);
            }
            catch (Exception ex)
            {
                errors.Add(_errorMapper.MapException(ex, "ARCHIVE_BATCH_CREATE"));
                return new ArchiveRunResponse
                {
                    Message = BuildFailureMessage(_localizer.Get("Archive.BatchCreationFailed"), errors),
                    Errors = errors,
                    ErrorCount = errors.Count
                };
            }

            foreach (var candidateId in candidateIds)
            {
                ArchiveRunItemResult item;
                try
                {
                    item = await executor.ExecuteAsync(candidateId, cancellationToken);
                }
                catch (Exception ex)
                {
                    errors.Add(_errorMapper.MapException(ex, "ARCHIVE_EXECUTE", ingestionFileId: candidateId));
                    item = new ArchiveRunItemResult
                    {
                        IngestionFileId = candidateId,
                        Status = "Failed",
                        Message = ex.InnerException != null
                            ? $"{ex.Message} | Inner: {ex.InnerException.Message}"
                            : ex.Message,
                        FailureReasons = new List<string> { "ARCHIVE_EXECUTION_FAILED" }
                    };
                }

                response.Items.Add(item);
                response.ProcessedCount++;

                switch (item.Status)
                {
                    case "Archived":
                        response.ArchivedCount++;
                        break;
                    case "Skipped":
                        response.SkippedCount++;
                        break;
                    default:
                        response.FailedCount++;
                        break;
                }

                try
                {
                    await executor.InsertBatchItemAsync(batchId.Value, item, cancellationToken);
                }
                catch (Exception ex)
                {
                    errors.Add(_errorMapper.MapException(ex, "ARCHIVE_BATCH_ITEM_INSERT", ingestionFileId: candidateId));
                }

                if (item.Status == "Failed" && !continueOnError)
                {
                    break;
                }
            }

            try
            {
                await executor.CompleteBatchAsync(batchId.Value, response, cancellationToken);
            }
            catch (Exception ex)
            {
                errors.Add(_errorMapper.MapException(ex, "ARCHIVE_BATCH_COMPLETE"));
            }

            response.Errors = errors;
            response.ErrorCount = errors.Count;
            return response;
        }
        catch (Exception ex)
        {
            errors.Add(_errorMapper.MapException(ex, "ARCHIVE_SERVICE_RUN"));
            return new ArchiveRunResponse
            {
                Message = BuildFailureMessage(_localizer.Get("Archive.RunFailed"), errors),
                Errors = errors,
                ErrorCount = errors.Count
            };
        }
    }

    private DateTime? ResolveBeforeDate(DateTime? requestBeforeDate)
    {
        if (_options.Defaults.UseConfiguredBeforeDateOnly.Value)
        {
            return ResolveConfiguredBeforeDate();
        }

        return requestBeforeDate ?? ResolveConfiguredBeforeDate();
    }

    private DateTime? ResolveConfiguredBeforeDate()
    {
        var strategy = _options.Defaults.DefaultBeforeDateStrategy;

        if (!SupportedBeforeDateStrategies.Contains(strategy))
        {
            throw new ArchiveBusinessException(
                "CONFIGURATION_ERROR",
                $"Unsupported DefaultBeforeDateStrategy: '{strategy}'. Supported values: {string.Join(", ", SupportedBeforeDateStrategies)}.");
        }

        return strategy switch
        {
            "RetentionDays" => DateTime.UtcNow.AddDays(-_options.Rules.RetentionDays.Value),
            "None" => null,
            _ => throw new ArchiveBusinessException(
                "CONFIGURATION_ERROR",
                $"Unsupported DefaultBeforeDateStrategy: '{strategy}'.")
        };
    }


    private static string BuildFailureMessage(string fallbackMessage, IReadOnlyCollection<ArchiveErrorDetail> errors)
    {
        var firstMessage = errors.FirstOrDefault()?.Message;
        if (string.IsNullOrWhiteSpace(firstMessage))
        {
            return fallbackMessage;
        }

        return $"{fallbackMessage} {firstMessage}";
    }

    private void EnsureAuditContext()
    {
        if (!string.IsNullOrWhiteSpace(_auditUserContextAccessor.CurrentUserId))
            return;

        var contextUserId = _contextProvider.CurrentContext?.UserId;
        _auditUserContextAccessor.CurrentUserId = !string.IsNullOrWhiteSpace(contextUserId)
            ? contextUserId
            : SystemUserId;
    }
}

using LinkPara.Card.Application.Commons.Interfaces.Archive;
using LinkPara.Card.Application.Commons.Models.Archive;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace LinkPara.Card.Infrastructure.Services.Archive;

internal sealed class ArchiveService : IArchiveService
{
    private readonly ArchiveAggregateReader _reader;
    private readonly ArchiveEligibilityEvaluator _evaluator;
    private readonly ArchiveExecutor _executor;
    private readonly ArchiveOptions _options;
    private readonly IArchiveErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;

    public ArchiveService(
        ArchiveAggregateReader reader,
        ArchiveEligibilityEvaluator evaluator,
        ArchiveExecutor executor,
        IOptions<ArchiveOptions> options,
        IArchiveErrorMapper errorMapper,
        Func<LinkPara.Card.Application.Commons.Localization.LocalizerResource, IStringLocalizer> localizerFactory)
    {
        _reader = reader;
        _evaluator = evaluator;
        _executor = executor;
        _options = options.Value;
        _errorMapper = errorMapper;
        _localizer = localizerFactory(LinkPara.Card.Application.Commons.Localization.LocalizerResource.Messages);
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
                ? _options.Defaults.PreviewLimit
                : effectiveRequest.Limit.Value;
            var candidateIds = await _reader.ResolveCandidateFileIdsAsync(
                effectiveRequest.AggregateIds ?? Array.Empty<Guid>(),
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
                        AggregateId = candidateId,
                        IsEligible = eligibility.IsEligible,
                        FailureReasons = eligibility.FailureReasons,
                        Counts = snapshot?.Counts ?? new ArchiveAggregateCounts()
                    });
                }
                catch (Exception ex)
                {
                    errors.Add(_errorMapper.MapException(ex, "ARCHIVE_PREVIEW_CANDIDATE", aggregateId: candidateId));
                    response.Candidates.Add(new ArchiveCandidateResult
                    {
                        AggregateId = candidateId,
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
        var errors = new List<ArchiveErrorDetail>();
        try
        {
            var effectiveRequest = request ?? new ArchiveRunRequest();
            var effectiveBeforeDate = ResolveBeforeDate(effectiveRequest.BeforeDate);
            var effectiveMaxFiles = effectiveRequest.MaxFiles is null or 0
                ? _options.Defaults.MaxRunCount
                : effectiveRequest.MaxFiles.Value;
            var continueOnError = effectiveRequest.ContinueOnError ?? _options.Defaults.ContinueOnError;
            var candidateIds = await _reader.ResolveCandidateFileIdsAsync(
                effectiveRequest.AggregateIds ?? Array.Empty<Guid>(),
                effectiveBeforeDate,
                effectiveMaxFiles,
                cancellationToken);

            var response = new ArchiveRunResponse();
            Guid? batchId = null;

            try
            {
                batchId = await _executor.CreateBatchAsync(effectiveRequest, cancellationToken);
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
                    item = await _executor.ExecuteAsync(candidateId, cancellationToken);
                }
                catch (Exception ex)
                {
                    errors.Add(_errorMapper.MapException(ex, "ARCHIVE_EXECUTE", aggregateId: candidateId));
                    item = new ArchiveRunItemResult
                    {
                        AggregateId = candidateId,
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
                    await _executor.InsertBatchItemAsync(batchId.Value, item, cancellationToken);
                }
                catch (Exception ex)
                {
                    errors.Add(_errorMapper.MapException(ex, "ARCHIVE_BATCH_ITEM_INSERT", aggregateId: candidateId));
                }

                if (item.Status == "Failed" && !continueOnError)
                {
                    break;
                }
            }

            try
            {
                await _executor.CompleteBatchAsync(batchId.Value, response, cancellationToken);
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
        if (_options.Defaults.UseConfiguredBeforeDateOnly)
        {
            return ResolveConfiguredBeforeDate();
        }

        return requestBeforeDate ?? ResolveConfiguredBeforeDate();
    }

    private DateTime? ResolveConfiguredBeforeDate()
    {
        return _options.Defaults.DefaultBeforeDateStrategy switch
        {
            "RetentionDays" => DateTime.UtcNow.AddDays(-Math.Abs(_options.Rules.RetentionDays)),
            "None" => null,
            _ => DateTime.UtcNow.AddDays(-Math.Abs(_options.Rules.RetentionDays))
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
}

using LinkPara.Card.Application.Commons.Interfaces.Archive;
using LinkPara.Card.Application.Commons.Models.Archive;
using Microsoft.Extensions.Options;

namespace LinkPara.Card.Infrastructure.Services.Archive;

internal sealed class ArchiveService : IArchiveService
{
    private readonly ArchiveAggregateReader _reader;
    private readonly ArchiveEligibilityEvaluator _evaluator;
    private readonly ArchiveExecutor _executor;
    private readonly ArchiveOptions _options;

    public ArchiveService(
        ArchiveAggregateReader reader,
        ArchiveEligibilityEvaluator evaluator,
        ArchiveExecutor executor,
        IOptions<ArchiveOptions> options)
    {
        _reader = reader;
        _evaluator = evaluator;
        _executor = executor;
        _options = options.Value;
    }

    public async Task<ArchivePreviewResponse> PreviewAsync(
        ArchivePreviewRequest request,
        CancellationToken cancellationToken = default)
    {
        var effectiveRequest = request ?? new ArchivePreviewRequest();
        var effectiveBeforeDate = ResolveBeforeDate(effectiveRequest.BeforeDate);
        var effectiveLimit = effectiveRequest.Limit ?? _options.Defaults.PreviewLimit;
        var candidateIds = await _reader.ResolveCandidateFileIdsAsync(
            effectiveRequest.AggregateIds ?? Array.Empty<Guid>(),
            effectiveBeforeDate,
            effectiveLimit,
            cancellationToken);

        var response = new ArchivePreviewResponse();
        foreach (var candidateId in candidateIds)
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

        return response;
    }

    public async Task<ArchiveRunResponse> RunAsync(
        ArchiveRunRequest request,
        CancellationToken cancellationToken = default)
    {
        var effectiveRequest = request ?? new ArchiveRunRequest();
        var effectiveBeforeDate = ResolveBeforeDate(effectiveRequest.BeforeDate);
        var effectiveMaxFiles = effectiveRequest.MaxFiles ?? _options.Defaults.MaxRunCount;
        var continueOnError = effectiveRequest.ContinueOnError ?? _options.Defaults.ContinueOnError;
        var candidateIds = await _reader.ResolveCandidateFileIdsAsync(
            effectiveRequest.AggregateIds ?? Array.Empty<Guid>(),
            effectiveBeforeDate,
            effectiveMaxFiles,
            cancellationToken);

        var response = new ArchiveRunResponse();
        var batchId = await _executor.CreateBatchAsync(effectiveRequest, cancellationToken);

        foreach (var candidateId in candidateIds)
        {
            var item = await _executor.ExecuteAsync(candidateId, cancellationToken);
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

            await _executor.InsertBatchItemAsync(batchId, item, cancellationToken);

            if (item.Status == "Failed" && !continueOnError)
            {
                break;
            }
        }

        await _executor.CompleteBatchAsync(batchId, response, cancellationToken);
        return response;
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
}

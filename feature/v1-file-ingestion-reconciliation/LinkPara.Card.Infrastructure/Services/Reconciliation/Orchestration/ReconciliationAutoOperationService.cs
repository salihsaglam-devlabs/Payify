using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Constants;
using LinkPara.Card.Application.Commons.Models.Logging;
using LinkPara.Card.Application.Commons.Models.Reconciliation;
using LinkPara.Card.Domain.Constants;
using LinkPara.Card.Domain.Enums;
using LinkPara.Card.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Orchestration;

public class ReconciliationAutoOperationService : IReconciliationAutoOperationService
{
    private readonly CardDbContext _dbContext;
    private readonly IReconciliationOperationExecutor _operationExecutor;
    private readonly IBulkServiceOperationLogPublisher _bulkLogPublisher;
    private readonly ILogger<ReconciliationAutoOperationService> _logger;

    public ReconciliationAutoOperationService(
        CardDbContext dbContext,
        IReconciliationOperationExecutor operationExecutor,
        IBulkServiceOperationLogPublisher bulkLogPublisher,
        ILogger<ReconciliationAutoOperationService> logger)
    {
        _dbContext = dbContext;
        _operationExecutor = operationExecutor;
        _bulkLogPublisher = bulkLogPublisher;
        _logger = logger;
    }

    public async Task<ReconciliationExecutionSummary> ExecutePendingOperationsAsync(
        int take = 100,
        string actor = null,
        CancellationToken cancellationToken = default)
    {
        return await RunWithBulkLogAsync(
            endpointName: nameof(ExecutePendingOperationsAsync),
            actor: actor,
            metadata: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["take"] = take.ToString()
            },
            run: async () =>
            {
                if (take <= 0)
                {
                    take = 100;
                }

                actor = string.IsNullOrWhiteSpace(actor) ? AuditUsers.CardFileIngestion : actor;
                var summary = new ReconciliationExecutionSummary { RequestedCount = take };

                var pendingRuns = await _dbContext.ReconciliationOperations
                    .AsNoTracking()
                    .Where(x => x.Mode == ReconciliationOperationMode.Auto &&
                                x.Status == ReconciliationOperationStatus.Pending)
                    .OrderBy(x => x.CreateDate)
                    .Select(x => new { x.CardTransactionRecordId, x.ClearingRecordId, x.RunId })
                    .Distinct()
                    .Take(take)
                    .ToListAsync(cancellationToken);

                foreach (var run in pendingRuns)
                {
                    try
                    {
                        var scope = new ReconciliationOperationScope
                        {
                            CardTransactionRecordId = run.CardTransactionRecordId,
                            ClearingRecordId = run.ClearingRecordId,
                            RunId = run.RunId
                        };

                        var strategy = _dbContext.Database.CreateExecutionStrategy();
                        var isRunExecutionSuccessful = await strategy.ExecuteAsync(async () =>
                        {
                            await using var tx = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
                            try
                            {
                                var plan = await _operationExecutor.PrepareAsync(scope, actor, cancellationToken);
                                var executed = plan is not null && await _operationExecutor.ExecuteAsync(plan, scope, actor, cancellationToken);
                                await UpdateCardStateFromRunAsync(run.CardTransactionRecordId, run.RunId, actor, cancellationToken);
                                await _dbContext.SaveChangesAsync(cancellationToken);
                                await tx.CommitAsync(cancellationToken);
                                return executed;
                            }
                            catch
                            {
                                await tx.RollbackAsync(cancellationToken);
                                throw;
                            }
                        });

                        summary.ProcessedCount++;
                        if (isRunExecutionSuccessful)
                        {
                            summary.SucceededCount++;
                        }
                        else
                        {
                            summary.FailedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        summary.ProcessedCount++;
                        summary.FailedCount++;
                        _logger.LogError(ex,
                            "Run execution failed and skipped. CardId={CardId}, RunId={RunId}",
                            run.CardTransactionRecordId,
                            run.RunId);
                    }
                }

                _logger.LogInformation("Reconciliation auto operations executed. Requested={Requested}, Processed={Processed}, Succeeded={Succeeded}, Failed={Failed}",
                    summary.RequestedCount, summary.ProcessedCount, summary.SucceededCount, summary.FailedCount);
                return summary;
            },
            cancellationToken);
    }

    private async Task<ReconciliationExecutionSummary> RunWithBulkLogAsync(
        string endpointName,
        string actor,
        Dictionary<string, string> metadata,
        Func<Task<ReconciliationExecutionSummary>> run,
        CancellationToken cancellationToken)
    {
        var startedAt = DateTime.Now;
        var steps = new List<string>();
        var success = false;
        ReconciliationExecutionSummary summary = null;
        try
        {
            summary = await run();
            success = summary?.FailedCount == 0;
            steps.Add($"Requested={summary?.RequestedCount ?? 0}");
            steps.Add($"Processed={summary?.ProcessedCount ?? 0}");
            steps.Add($"Succeeded={summary?.SucceededCount ?? 0}");
            steps.Add($"Failed={summary?.FailedCount ?? 0}");
            return summary;
        }
        catch (Exception ex)
        {
            steps.Add($"Exception={ex.Message}");
            _logger.LogError(ex, "Auto operation endpoint failed. Endpoint={EndpointName}", endpointName);
            throw;
        }
        finally
        {
            var endedAt = DateTime.Now;
            await _bulkLogPublisher.PublishAsync(new BulkServiceOperationLog
            {
                ServiceName = nameof(ReconciliationAutoOperationService),
                EndpointName = endpointName,
                Actor = string.IsNullOrWhiteSpace(actor) ? AuditUsers.CardFileIngestion : actor,
                StartedAt = startedAt,
                EndedAt = endedAt,
                DurationMs = (long)(endedAt - startedAt).TotalMilliseconds,
                IsSuccess = success,
                Summary = success ? "Auto operation execution completed." : "Auto operation execution failed.",
                Metadata = metadata ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                Logs = steps
            }, cancellationToken);
        }
    }

    private async Task UpdateCardStateFromRunAsync(
        Guid cardTransactionRecordId,
        Guid runId,
        string actor,
        CancellationToken cancellationToken)
    {
        var operations = await _dbContext.ReconciliationOperations
            .AsNoTracking()
            .Where(x => x.CardTransactionRecordId == cardTransactionRecordId && x.RunId == runId)
            .ToListAsync(cancellationToken);
        var executionGroupId = operations.Select(x => (Guid?)x.ExecutionGroupId).FirstOrDefault();
        var operationIds = operations.Select(x => x.Id).ToArray();
        var hasPendingManualReview = operationIds.Length > 0 && await _dbContext.ReconciliationManualReviewItems
            .AsNoTracking()
            .AnyAsync(x => operationIds.Contains(x.ReconciliationOperationId) && x.ReviewStatus == ManualReviewStatus.Pending, cancellationToken);

        var (state, reason) = CardReconciliationStateResolver.Resolve(operations, hasPendingManualReview);
        var now = DateTime.Now;
        await _dbContext.CardTransactionRecords
            .Where(x => x.Id == cardTransactionRecordId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.ReconciliationState, state)
                .SetProperty(x => x.ReconciliationStateReason, reason)
                .SetProperty(x => x.LastReconciliationRunId, runId)
                .SetProperty(x => x.LastReconciliationExecutionGroupId, executionGroupId)
                .SetProperty(x => x.ReconciliationStateUpdatedAt, now)
                .SetProperty(x => x.UpdateDate, now)
                .SetProperty(x => x.LastModifiedBy, actor), cancellationToken);
    }
}

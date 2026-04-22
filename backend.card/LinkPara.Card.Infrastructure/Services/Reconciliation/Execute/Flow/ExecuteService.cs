using System.Text.Json;
using LinkPara.Card.Application.Commons.Extensions;
using LinkPara.Card.Application.Commons.Helpers;
using LinkPara.Card.Application.Commons.Interfaces;
using Microsoft.Extensions.Localization;
using LinkPara.Card.Application.Commons.Interfaces.Reconciliation;
using LinkPara.Card.Application.Commons.Models.AppConfiguration;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Contracts.Requests;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Contracts.Responses;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Shared;
using LinkPara.Card.Domain.Entities.Reconciliation.Persistence;
using LinkPara.Card.Domain.Enums.Reconciliation;
using LinkPara.Card.Infrastructure.Persistence;
using LinkPara.Card.Infrastructure.Services.Alert;
using LinkPara.Card.Infrastructure.Services.Audit;
using LinkPara.Card.Infrastructure.Services.Reconciliation.Evaluate.Core;
using LinkPara.Card.Infrastructure.Services.Reconciliation.Execute.Core;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Execute.Flow;

internal sealed class ExecuteService
{
    private const int BaseRetrySeconds = 30;
    private static readonly int MaxEvaluationParallelism =
        Math.Clamp(Environment.ProcessorCount * 2, 4, 16);
    private static readonly string WorkerLeaseOwner = BuildWorkerLeaseOwner();

    private readonly CardDbContext _dbContext;
    private readonly OperationExecutor _operationExecutor;
    private readonly IAlertService _alertService;
    private readonly IAuditStampService _auditStampService;
    private readonly IReconciliationErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ITimeProvider _timeProvider;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly CardConfigOptions.OperationsExecuteEndpoint _options;

    public ExecuteService(
        CardDbContext dbContext,
        OperationExecutor operationExecutor,
        IAlertService alertService,
        IAuditStampService auditStampService,
        ITimeProvider timeProvider,
        IOptions<CardConfigOptions> options,
        IReconciliationErrorMapper errorMapper,
        IServiceScopeFactory serviceScopeFactory,
        Func<LinkPara.Card.Application.Commons.Localization.LocalizerResource, IStringLocalizer> localizerFactory)
    {
        _dbContext = dbContext;
        _operationExecutor = operationExecutor;
        _alertService = alertService;
        _auditStampService = auditStampService;
        _timeProvider = timeProvider;
        _options = options.Value.Endpoints.Reconciliation.OperationsExecute;
        _errorMapper = errorMapper;
        _serviceScopeFactory = serviceScopeFactory;
        _localizer = localizerFactory(LinkPara.Card.Application.Commons.Localization.LocalizerResource.Messages);
    }

    public async Task<ExecuteResponse> ExecuteAsync(
        ExecuteRequest request,
        List<ReconciliationErrorDetail>? errors = null,
        CancellationToken cancellationToken = default)
    {
        errors ??= new List<ReconciliationErrorDetail>();
        var now = _timeProvider.Now;
        var executeOptions = ResolveExecuteOptions(request);
        var selection = ExecutionSelection.Create(request);

        var response = new ExecuteResponse
        {
            Errors = errors
        };

        var maxEvaluations = executeOptions.MaxEvaluations.Value;
        var leaseSeconds = executeOptions.LeaseSeconds.Value;

        var evaluationIds = await ResolveTargetEvaluationIdsAsync(
            selection,
            now,
            maxEvaluations,
            cancellationToken);

        if (evaluationIds.Length == 0)
        {
            await RunAlertServiceSafelyAsync(errors, cancellationToken);
            response.ErrorCount = errors.Count;
            return response;
        }
        
        var remaining = maxEvaluations;
        var remainingLock = new object();
        var resultsLock = new object();
        var errorsLock = new object();

        var dop = Math.Min(MaxEvaluationParallelism, evaluationIds.Length);
        if (dop <= 1)
        {
            await ExecuteEvaluationsSequentiallyAsync(
                evaluationIds, selection, now, leaseSeconds,
                response, errors, () => remaining,
                consumed => remaining = Math.Max(0, remaining - consumed),
                cancellationToken);
        }
        else
        {
            await Parallel.ForEachAsync(
                evaluationIds,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = dop,
                    CancellationToken = cancellationToken
                },
                async (evaluationId, ct) =>
                {
                    int budget;
                    lock (remainingLock)
                    {
                        budget = remaining;
                    }

                    if (budget <= 0)
                    {
                        return;
                    }

                    using var scope = _serviceScopeFactory.CreateScope();
                    var worker = scope.ServiceProvider.GetRequiredService<ExecuteService>();

                    List<OperationExecutionResult> evalResults;
                    try
                    {
                        evalResults = await worker.RunSingleEvaluationAsync(
                            evaluationId,
                            now,
                            budget,
                            leaseSeconds,
                            selection,
                            errors,
                            errorsLock,
                            ct);
                    }
                    catch (OperationCanceledException) when (ct.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        lock (errorsLock)
                        {
                            errors.Add(_errorMapper.MapException(
                                ex,
                                "EXECUTION_EVALUATION",
                                evaluationId: evaluationId,
                                message: _localizer.Get("Reconciliation.EvaluationExecutionFailed", evaluationId)));
                        }
                        return;
                    }

                    lock (resultsLock)
                    {
                        foreach (var result in evalResults)
                        {
                            UpdateExecutionTotals(response, result);

                            if (IsFailedResult(result))
                            {
                                response.Results.Add(result);
                            }
                        }
                    }

                    lock (remainingLock)
                    {
                        remaining = Math.Max(0, maxEvaluations - response.TotalAttempted);
                    }
                });
        }

        await RunAlertServiceSafelyAsync(errors, cancellationToken);
        response.ErrorCount = errors.Count;
        return response;
    }

    private async Task ExecuteEvaluationsSequentiallyAsync(
        Guid[] evaluationIds,
        ExecutionSelection selection,
        DateTime now,
        int leaseSeconds,
        ExecuteResponse response,
        List<ReconciliationErrorDetail> errors,
        Func<int> getRemaining,
        Action<int> consumeRemaining,
        CancellationToken cancellationToken)
    {
        foreach (var evaluationId in evaluationIds)
        {
            var remaining = getRemaining();
            if (remaining <= 0)
            {
                break;
            }

            List<OperationExecutionResult> results;
            try
            {
                results = await ExecuteEvaluationAsync(
                    evaluationId,
                    now,
                    remaining,
                    leaseSeconds,
                    selection,
                    errors,
                    cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                errors.Add(_errorMapper.MapException(
                    ex,
                    "EXECUTION_EVALUATION",
                    evaluationId: evaluationId,
                    message: _localizer.Get("Reconciliation.EvaluationExecutionFailed", evaluationId)));
                continue;
            }

            foreach (var result in results)
            {
                UpdateExecutionTotals(response, result);

                if (IsFailedResult(result))
                {
                    response.Results.Add(result);
                }
            }

            consumeRemaining(results.Count);
        }
    }
    
    private async Task<List<OperationExecutionResult>> RunSingleEvaluationAsync(
        Guid evaluationId,
        DateTime now,
        int remainingLimit,
        int leaseSeconds,
        ExecutionSelection selection,
        List<ReconciliationErrorDetail> sharedErrors,
        object sharedErrorsLock,
        CancellationToken cancellationToken)
    {
        var localErrors = new List<ReconciliationErrorDetail>();
        try
        {
            return await ExecuteEvaluationAsync(
                evaluationId,
                now,
                remainingLimit,
                leaseSeconds,
                selection,
                localErrors,
                cancellationToken);
        }
        finally
        {
            if (localErrors.Count > 0)
            {
                lock (sharedErrorsLock)
                {
                    sharedErrors.AddRange(localErrors);
                }
            }
        }
    }

    private async Task RunAlertServiceSafelyAsync(
        List<ReconciliationErrorDetail> errors,
        CancellationToken cancellationToken)
    {
        try
        {
            await _alertService.ExecuteAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            errors.Add(_errorMapper.MapException(ex, "RECONCILIATION_ALERT_SERVICE_EXECUTE"));
        }
    }

    private async Task<Guid[]> ResolveTargetEvaluationIdsAsync(
        ExecutionSelection selection,
        DateTime now,
        int maxEvaluations,
        CancellationToken cancellationToken)
    {

        var query = _dbContext.ReconciliationOperations
            .AsNoTracking()
            .Where(x =>
                x.Status == OperationStatus.Planned ||
                x.Status == OperationStatus.Blocked ||
                x.Status == OperationStatus.Executing)
            .Where(x => !x.NextAttemptAt.HasValue || x.NextAttemptAt.Value <= now)
            .Where(x => !x.LeaseExpiresAt.HasValue || x.LeaseExpiresAt.Value <= now);

        if (selection.Mode == ExecutionSelectionMode.OperationIds)
        {
            query = query.Where(x => selection.OperationIds.Contains(x.Id));
        }
        else if (selection.Mode == ExecutionSelectionMode.EvaluationIds)
        {
            query = query.Where(x => selection.EvaluationIds.Contains(x.EvaluationId));
        }
        else if (selection.Mode == ExecutionSelectionMode.GroupIds)
        {
            query = query.Where(x => selection.GroupIds.Contains(x.GroupId));
        }
        
        return await query
            .Select(x => x.EvaluationId)
            .Distinct()
            .OrderBy(id => id)
            .Take(maxEvaluations)
            .ToArrayAsync(cancellationToken);
    }

    private async Task<List<OperationExecutionResult>> ExecuteEvaluationAsync(
        Guid evaluationId,
        DateTime now,
        int remainingLimit,
        int leaseSeconds,
        ExecutionSelection selection,
        List<ReconciliationErrorDetail> errors,
        CancellationToken cancellationToken)
    {
        var results = new List<OperationExecutionResult>();
        
        _dbContext.ChangeTracker.Clear();
        var operations = await LoadEvaluationOperationWindowAsync(evaluationId, cancellationToken);

        while (results.Count < remainingLimit)
        {
            if (operations.Count == 0)
            {
                break;
            }

            var nextOperation = GetNextOperation(operations, now, selection);

            if (nextOperation is null)
            {
                break;
            }

            if (!await TryClaimOperationAsync(nextOperation, now, leaseSeconds, cancellationToken))
            {
                _dbContext.ChangeTracker.Clear();
                operations = await LoadEvaluationOperationWindowAsync(evaluationId, cancellationToken);
                continue;
            }

            var result = await ExecuteOperationAsync(
                nextOperation,
                operations,
                now,
                errors,
                cancellationToken);

            results.Add(result);

            if (ShouldStopExecutingEvaluation(result))
            {
                break;
            }
        }

        return results;
    }
    
    private async Task<List<ReconciliationOperation>> LoadEvaluationOperationWindowAsync(
        Guid evaluationId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.ReconciliationOperations
            .AsTracking()
            .Where(x => x.EvaluationId == evaluationId)
            .OrderBy(x => x.SequenceNumber)
            .ToListAsync(cancellationToken);
    }

    private static ReconciliationOperation? GetNextOperation(
        IReadOnlyList<ReconciliationOperation> operations,
        DateTime now,
        ExecutionSelection selection)
    {
        foreach (var operation in operations)
        {
            if (operation.Status is OperationStatus.Completed or OperationStatus.Cancelled or OperationStatus.Failed)
            {
                continue;
            }

            if (!selection.Matches(operation))
            {
                continue;
            }

            if (operation.Status == OperationStatus.Executing)
            {
                if (!HasExpiredLease(operation, now))
                {
                    return null;
                }

            }
            else if (operation.Status == OperationStatus.Blocked)
            {
                if (!CanPromoteToPlanned(operation, operations))
                {
                    return null;
                }

            }
            else if (operation.Status != OperationStatus.Planned)
            {
                return null;
            }

            if (operation.NextAttemptAt.HasValue && operation.NextAttemptAt.Value > now)
            {
                return null;
            }

            if (operation.LeaseExpiresAt.HasValue && operation.LeaseExpiresAt.Value > now)
            {
                return null;
            }

            return operation;
        }

        return null;
    }

    private static bool CanPromoteToPlanned(
        ReconciliationOperation operation,
        IReadOnlyList<ReconciliationOperation> operations)
    {
        if (IsRootOperation(operation))
        {
            return true;
        }

        if (HasEarlierPendingOperation(operation, operations))
        {
            return false;
        }

        return HasCompletedParent(operation, operations);
    }

    private static bool HasEarlierPendingOperation(
        ReconciliationOperation operation,
        IReadOnlyList<ReconciliationOperation> operations)
    {
        return operations.Any(x =>
            x.SequenceNumber < operation.SequenceNumber &&
            x.Status != OperationStatus.Completed &&
            x.Status != OperationStatus.Cancelled &&
            x.Status != OperationStatus.Failed);
    }

    private static bool HasExpiredLease(ReconciliationOperation operation, DateTime now)
    {
        return operation.LeaseExpiresAt.HasValue && operation.LeaseExpiresAt.Value <= now;
    }


    private static bool IsRootOperation(ReconciliationOperation operation)
    {
        return operation.SequenceNumber == 0 && !operation.ParentSequenceNumber.HasValue;
    }

    private static bool HasCompletedParent(
        ReconciliationOperation operation,
        IReadOnlyList<ReconciliationOperation> operations)
    {
        if (!operation.ParentSequenceNumber.HasValue)
        {
            return false;
        }

        var parentOperation = operations.SingleOrDefault(x => x.SequenceNumber == operation.ParentSequenceNumber.Value);
        return parentOperation?.Status == OperationStatus.Completed;
    }

    private async Task<bool> TryClaimOperationAsync(
        ReconciliationOperation operation,
        DateTime now,
        int leaseSeconds,
        CancellationToken cancellationToken)
    {
        var leaseExpiresAt = now.AddSeconds(leaseSeconds);
        var auditStamp = _auditStampService.CreateStamp();

        var updatedRows = await _dbContext.ReconciliationOperations
            .Where(x => x.Id == operation.Id)
            .Where(x =>
                x.Status == OperationStatus.Planned ||
                x.Status == OperationStatus.Blocked ||
                (x.Status == OperationStatus.Executing &&
                 (x.LeaseExpiresAt == null || x.LeaseExpiresAt <= now)))
            .Where(x => x.NextAttemptAt == null || x.NextAttemptAt <= now)
            .Where(x => x.LeaseExpiresAt == null || x.LeaseExpiresAt <= now)
            .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.Status, OperationStatus.Executing)
                    .SetProperty(x => x.LeaseOwner, WorkerLeaseOwner)
                    .SetProperty(x => x.LeaseExpiresAt, leaseExpiresAt)
                    .SetProperty(x => x.UpdateDate, auditStamp.Timestamp)
                    .SetProperty(x => x.LastModifiedBy, auditStamp.UserId),
                cancellationToken);

        if (updatedRows == 0)
        {
            return false;
        }
        
        operation.Status = OperationStatus.Executing;
        operation.LeaseOwner = WorkerLeaseOwner;
        operation.LeaseExpiresAt = leaseExpiresAt;
        operation.UpdateDate = auditStamp.Timestamp;
        operation.LastModifiedBy = auditStamp.UserId;
        return true;
    }

    private static string BuildWorkerLeaseOwner()
    {
        var machineName = Environment.MachineName;
        var processId = Environment.ProcessId;
        return $"exec:{machineName}:{processId}";
    }

    private async Task<OperationExecutionResult> ExecuteOperationAsync(
        ReconciliationOperation operation,
        IReadOnlyList<ReconciliationOperation> evaluationOperations,
        DateTime now,
        List<ReconciliationErrorDetail> errors,
        CancellationToken cancellationToken)
    {
        var attemptNumber = operation.RetryCount + 1;

        var execution = new ReconciliationOperationExecution
        {
            Id = Guid.NewGuid(),
            FileLineId = operation.FileLineId,
            GroupId = operation.GroupId,
            EvaluationId = operation.EvaluationId,
            OperationId = operation.Id,
            AttemptNumber = attemptNumber,
            StartedAt = now,
            Status = ExecutionStatus.Started,
            RequestPayload = operation.Payload ?? string.Empty,
            ResponsePayload = string.Empty,
            ResultCode = string.Empty,
            ResultMessage = string.Empty,
            ErrorCode = string.Empty,
            ErrorMessage = string.Empty
        };

        _auditStampService.StampForCreate(execution);
        _dbContext.ReconciliationOperationExecutions.Add(execution);

        try
        {
            if (string.Equals(operation.Code, OperationCodes.CreateManualReview, StringComparison.Ordinal))
            {
                return await ExecuteManualGateAsync(
                    operation,
                    evaluationOperations,
                    execution,
                    now,
                    errors,
                    cancellationToken);
            }

            var alreadyApplied = !string.IsNullOrEmpty(operation.IdempotencyKey) &&
                                 await _dbContext.ReconciliationOperations
                                     .AsNoTracking()
                                     .AnyAsync(x =>
                                             x.Id != operation.Id &&
                                             x.IdempotencyKey == operation.IdempotencyKey &&
                                             x.Status == OperationStatus.Completed,
                                         cancellationToken);

            if (alreadyApplied)
            {
                operation.Status = OperationStatus.Completed;

                return await CompleteExecutionAsync(
                    operation,
                    execution,
                    now,
                    ExecutionStatus.Skipped,
                    "SKIPPED_ALREADY_APPLIED",
                    _localizer.Get("Reconciliation.OperationAlreadyApplied"),
                    "Skipped",
                    cancellationToken);
            }

            var handlerResult = await _operationExecutor.ExecuteAsync(operation, cancellationToken);
            ReleaseLease(operation);

            if (handlerResult.IsSuccessful)
            {
                operation.Status = OperationStatus.Completed;
                operation.LastError = null;
                operation.NextAttemptAt = null;
            }
            else
            {
                ScheduleRetry(operation, now, handlerResult.ErrorMessage);

                errors.Add(_errorMapper.Create(
                    "OPERATION_EXECUTION_FAILED",
                    handlerResult.ErrorMessage ??
                    handlerResult.ResultMessage ??
                    _localizer.Get("Reconciliation.OperationFailed", operation.Code),
                    "EXECUTION_OPERATION",
                    fileLineId: operation.FileLineId,
                    operationId: operation.Id,
                    evaluationId: operation.EvaluationId,
                    detail: _localizer.Get("Reconciliation.OperationAttemptDetail", operation.Code, attemptNumber)));
            }

            execution.Status = handlerResult.IsSkipped
                ? ExecutionStatus.Skipped
                : handlerResult.IsSuccessful
                    ? ExecutionStatus.Completed
                    : ExecutionStatus.Failed;

            execution.FinishedAt = _timeProvider.Now;
            execution.RequestPayload = JsonSerializer.Serialize(handlerResult.RequestPayload);
            execution.ResponsePayload = JsonSerializer.Serialize(handlerResult.ResponsePayload);
            execution.ResultCode = handlerResult.ResultCode;
            execution.ResultMessage = handlerResult.ResultMessage;
            execution.ErrorCode = handlerResult.ErrorCode ?? string.Empty;
            execution.ErrorMessage = handlerResult.ErrorMessage ?? string.Empty;

            if (!handlerResult.IsSuccessful && operation.Status == OperationStatus.Failed)
            {
                await AddOperationFailureAlertAsync(
                    operation,
                    handlerResult.ErrorMessage ?? handlerResult.ResultMessage,
                    cancellationToken);
            }

            _auditStampService.StampForUpdate(new AuditEntity[] { operation, execution });
            await _dbContext.SaveChangesAsync(cancellationToken);

            return CreateOperationResult(
                operation.Id,
                handlerResult.IsSkipped ? "Skipped" : handlerResult.IsSuccessful ? "Completed" : "Failed",
                handlerResult.ResultMessage);
        }
        catch (Exception ex)
        {
            errors.Add(_errorMapper.MapException(
                ex,
                "EXECUTION_OPERATION",
                fileLineId: operation.FileLineId,
                operationId: operation.Id,
                evaluationId: operation.EvaluationId,
                message: _localizer.Get("Reconciliation.UnexpectedOperationError", operation.Code)));

            ScheduleRetry(operation, now, ExceptionDetailHelper.BuildDetailMessage(ex, 2000));

            execution.Status = ExecutionStatus.Failed;
            execution.FinishedAt = _timeProvider.Now;
            execution.ResultCode = "FAILED";
            execution.ResultMessage = _localizer.Get("Reconciliation.OperationExecutionFailed");
            execution.ErrorCode = "OPERATION_EXECUTION_FAILED";
            execution.ErrorMessage = ExceptionDetailHelper.BuildDetailMessage(ex, 2000);

            if (operation.Status == OperationStatus.Failed)
            {
                await AddOperationFailureAlertAsync(operation, ExceptionDetailHelper.BuildDetailMessage(ex, 2000), cancellationToken);
            }

            _auditStampService.StampForUpdate(new AuditEntity[] { operation, execution });
            await _dbContext.SaveChangesAsync(cancellationToken);

            return CreateOperationResult(operation.Id, "Failed", ExceptionDetailHelper.BuildDetailMessage(ex, 2000));
        }
    }

    private async Task<OperationExecutionResult> ExecuteManualGateAsync(
        ReconciliationOperation operation,
        IReadOnlyList<ReconciliationOperation> evaluationOperations,
        ReconciliationOperationExecution execution,
        DateTime now,
        List<ReconciliationErrorDetail> errors,
        CancellationToken cancellationToken)
    {
        var review = await _dbContext.ReconciliationReviews
            .AsTracking()
            .SingleOrDefaultAsync(x => x.OperationId == operation.Id, cancellationToken);

        if (review is null)
        {
            errors.Add(_errorMapper.Create(
                "MANUAL_REVIEW_NOT_FOUND",
                _localizer.Get("Reconciliation.ManualReviewNotResolved"),
                "EXECUTION_MANUAL_GATE",
                fileLineId: operation.FileLineId,
                operationId: operation.Id,
                evaluationId: operation.EvaluationId));

            operation.Status = OperationStatus.Failed;
            ReleaseLease(operation);
            operation.LastError = _localizer.Get("Reconciliation.ManualReviewNotResolved");

            execution.Status = ExecutionStatus.Failed;
            execution.FinishedAt = now;
            execution.ResultCode = "MANUAL_REVIEW_NOT_FOUND";
            execution.ResultMessage = _localizer.Get("Reconciliation.ManualReviewNotResolved");
            execution.ErrorCode = "MANUAL_REVIEW_NOT_FOUND";
            execution.ErrorMessage = _localizer.Get("Reconciliation.ManualReviewNotResolved");

            _auditStampService.StampForUpdate(new AuditEntity[] { operation, execution });
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new OperationExecutionResult
            {
                OperationId = operation.Id,
                Status = "Failed",
                Message = _localizer.Get("Reconciliation.ManualReviewNotResolved")
            };
        }

        if (review.Decision == ReviewDecision.Pending &&
            review.ExpiresAt.HasValue &&
            review.ExpiresAt.Value <= now)
        {
            ApplyExpirationDecision(review, now);
        }

        if (review.Decision == ReviewDecision.Pending)
        {
            operation.Status = OperationStatus.Blocked;
            ReleaseLease(operation);
            operation.NextAttemptAt = review.ExpiresAt ?? now.AddSeconds(BaseRetrySeconds);

            execution.Status = ExecutionStatus.Skipped;
            execution.FinishedAt = now;
            execution.ResultCode = "WAITING_MANUAL_DECISION";
            execution.ResultMessage = _localizer.Get("Reconciliation.ManualReviewDecisionPending");
            execution.ResponsePayload = JsonSerializer.Serialize(new { review.Decision, review.ExpiresAt });

            _auditStampService.StampForUpdate(new AuditEntity[] { operation, execution });
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new OperationExecutionResult
            {
                OperationId = operation.Id,
                Status = "Blocked",
                Message = _localizer.Get("Reconciliation.ManualReviewDecisionPending")
            };
        }

        ResolveManualGate(operation, evaluationOperations, review);
        ReleaseLease(operation);
        operation.LastError = null;

        execution.Status = ExecutionStatus.Completed;
        execution.FinishedAt = now;
        execution.ResultCode = $"MANUAL_GATE_{review.Decision.ToString().ToUpperInvariant()}";
        execution.ResultMessage = _localizer.Get("Reconciliation.ManualGateResolved", review.Decision);
        execution.ResponsePayload = JsonSerializer.Serialize(new { review.Decision });

        var manualGateEntities = evaluationOperations
            .Cast<AuditEntity>()
            .Append(review)
            .Append(execution)
            .ToArray();
        _auditStampService.StampForUpdate(manualGateEntities);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new OperationExecutionResult
        {
            OperationId = operation.Id,
            Status = operation.Status == OperationStatus.Cancelled ? "Skipped" : "Completed",
            Message = execution.ResultMessage
        };
    }

    private static void ResolveManualGate(
        ReconciliationOperation gateOperation,
        IReadOnlyList<ReconciliationOperation> evaluationOperations,
        ReconciliationReview review)
    {
        var branchOperations = evaluationOperations
            .Where(x => x.ParentSequenceNumber == gateOperation.SequenceNumber &&
                        (x.Branch == Branches.Approve || x.Branch == Branches.Reject))
            .ToList();

        switch (review.Decision)
        {
            case ReviewDecision.Approved:
                gateOperation.Status = OperationStatus.Completed;
                SetBranchStatuses(branchOperations, Branches.Approve);
                break;

            case ReviewDecision.Rejected:
                gateOperation.Status = OperationStatus.Completed;
                SetBranchStatuses(branchOperations, Branches.Reject);
                break;

            case ReviewDecision.Cancelled:
                gateOperation.Status = OperationStatus.Cancelled;

                foreach (var operation in branchOperations)
                {
                    operation.Status = OperationStatus.Cancelled;
                }

                if (review.ExpirationFlowAction == ReviewExpirationFlowAction.CancelRemaining)
                {
                    foreach (var operation in evaluationOperations.Where(x =>
                                 x.SequenceNumber > gateOperation.SequenceNumber &&
                                 x.Status != OperationStatus.Completed &&
                                 x.Status != OperationStatus.Failed))
                    {
                        operation.Status = OperationStatus.Cancelled;
                    }
                }

                break;
        }
    }

    private static void SetBranchStatuses(IEnumerable<ReconciliationOperation> branchOperations, string activeBranch)
    {
        foreach (var operation in branchOperations)
        {
            operation.Status = operation.Branch == activeBranch
                ? OperationStatus.Blocked
                : OperationStatus.Cancelled;
        }
    }

    private static void ApplyExpirationDecision(ReconciliationReview review, DateTime decisionAtUtc)
    {
        review.Decision = review.ExpirationAction switch
        {
            ReviewExpirationAction.Approve => ReviewDecision.Approved,
            ReviewExpirationAction.Reject => ReviewDecision.Rejected,
            _ => ReviewDecision.Cancelled
        };

        review.DecisionAt ??= decisionAtUtc;
    }

    private async Task<OperationExecutionResult> CompleteExecutionAsync(
        ReconciliationOperation operation,
        ReconciliationOperationExecution execution,
        DateTime now,
        ExecutionStatus executionStatus,
        string resultCode,
        string resultMessage,
        string responseStatus,
        CancellationToken cancellationToken)
    {
        operation.Status = OperationStatus.Completed;
        operation.LastError = null;
        operation.LeaseExpiresAt = null;
        operation.LeaseOwner = null;
        operation.NextAttemptAt = null;

        execution.Status = executionStatus;
        execution.FinishedAt = now;
        execution.ResultCode = resultCode;
        execution.ResultMessage = resultMessage;
        execution.ResponsePayload = JsonSerializer.Serialize(new { resultCode, resultMessage });

        _auditStampService.StampForUpdate(new AuditEntity[] { operation, execution });
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new OperationExecutionResult
        {
            OperationId = operation.Id,
            Status = responseStatus,
            Message = resultMessage
        };
    }

    private CardConfigOptions.OperationsExecuteEndpoint ResolveExecuteOptions(ExecuteRequest request)
    {
        return request.Options ?? _options;
    }

    private static void UpdateExecutionTotals(
        ExecuteResponse response,
        OperationExecutionResult result)
    {
        response.TotalAttempted++;

        if (IsSuccessfulResult(result))
        {
            response.TotalSucceeded++;
            return;
        }

        if (IsFailedResult(result))
        {
            response.TotalFailed++;
        }
    }

    private static bool IsSuccessfulResult(OperationExecutionResult result)
    {
        return string.Equals(result.Status, "Completed", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(result.Status, "Skipped", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsFailedResult(OperationExecutionResult result)
    {
        return string.Equals(result.Status, "Failed", StringComparison.OrdinalIgnoreCase);
    }

    private static bool ShouldStopExecutingEvaluation(OperationExecutionResult result)
    {
        return string.Equals(result.Status, "Blocked", StringComparison.OrdinalIgnoreCase) ||
               IsFailedResult(result);
    }

    private static void ReleaseLease(ReconciliationOperation operation)
    {
        operation.LeaseExpiresAt = null;
        operation.LeaseOwner = null;
    }

    private static void ScheduleRetry(
        ReconciliationOperation operation,
        DateTime now,
        string? lastError)
    {
        operation.RetryCount += 1;
        ReleaseLease(operation);
        operation.LastError = lastError;
        operation.Status = operation.RetryCount >= operation.MaxRetries
            ? OperationStatus.Failed
            : OperationStatus.Planned;

        operation.NextAttemptAt = operation.Status == OperationStatus.Planned
            ? now.AddSeconds((int)(BaseRetrySeconds * Math.Pow(2, operation.RetryCount)))
            : null;
    }

    private async Task AddOperationFailureAlertAsync(
        ReconciliationOperation operation,
        string? message,
        CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        var alert = new ReconciliationAlert
        {
            Id = Guid.NewGuid(),
            FileLineId = operation.FileLineId,
            GroupId = operation.GroupId, // kept for informational purposes only
            EvaluationId = operation.EvaluationId,
            OperationId = operation.Id,
            Severity = "Error",
            AlertType = "OperationExecutionFailed",
            Message = message
        };
        _auditStampService.StampForCreate(alert);
        _dbContext.ReconciliationAlerts.Add(alert);
    }

    private static OperationExecutionResult CreateOperationResult(
        Guid operationId,
        string status,
        string? message)
    {
        return new OperationExecutionResult
        {
            OperationId = operationId,
            Status = status,
            Message = message
        };
    }

    private sealed class ExecutionSelection
    {
        public static readonly ExecutionSelection All = new([], [], [], ExecutionSelectionMode.All);

        public HashSet<Guid> GroupIds { get; }
        public HashSet<Guid> EvaluationIds { get; }
        public HashSet<Guid> OperationIds { get; }
        public ExecutionSelectionMode Mode { get; }

        private ExecutionSelection(
            HashSet<Guid> groupIds,
            HashSet<Guid> evaluationIds,
            HashSet<Guid> operationIds,
            ExecutionSelectionMode mode)
        {
            GroupIds = groupIds;
            EvaluationIds = evaluationIds;
            OperationIds = operationIds;
            Mode = mode;
        }

        public static ExecutionSelection Create(ExecuteRequest request)
        {
            var operationIds = request.OperationIds
                .Where(x => x != Guid.Empty)
                .ToHashSet();
            if (operationIds.Count > 0)
            {
                return new ExecutionSelection([], [], operationIds, ExecutionSelectionMode.OperationIds);
            }

            var evaluationIds = request.EvaluationIds
                .Where(x => x != Guid.Empty)
                .ToHashSet();
            if (evaluationIds.Count > 0)
            {
                return new ExecutionSelection([], evaluationIds, [], ExecutionSelectionMode.EvaluationIds);
            }

            var groupIds = request.GroupIds
                .Where(x => x != Guid.Empty)
                .ToHashSet();
            if (groupIds.Count > 0)
            {
                return new ExecutionSelection(groupIds, [], [], ExecutionSelectionMode.GroupIds);
            }

            return All;
        }

        public bool Matches(ReconciliationOperation operation)
        {
            return Mode switch
            {
                ExecutionSelectionMode.OperationIds => OperationIds.Contains(operation.Id),
                ExecutionSelectionMode.EvaluationIds => EvaluationIds.Contains(operation.EvaluationId),
                ExecutionSelectionMode.GroupIds => GroupIds.Contains(operation.GroupId),
                _ => true
            };
        }
    }

    private enum ExecutionSelectionMode
    {
        All = 0,
        GroupIds = 1,
        EvaluationIds = 2,
        OperationIds = 3
    }

}

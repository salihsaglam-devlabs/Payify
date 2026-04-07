using System.Text.Json;
using LinkPara.Card.Application.Commons.Helpers.Reconciliation;
using LinkPara.Card.Application.Commons.Models.Reconciliation;
using LinkPara.Card.Domain.Entities.Reconciliation;
using LinkPara.Card.Domain.Enums.Reconciliation;
using LinkPara.Card.Infrastructure.Persistence;
using LinkPara.Card.Infrastructure.Services.Audit;
using LinkPara.Card.Infrastructure.Services.AlertService;
using LinkPara.Card.Infrastructure.Services.Reconciliation.Execute;
using LinkPara.Card.Infrastructure.Services.Reconciliation.Evaluate;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Execute.Flow;

internal sealed class ExecuteService
{
    private const int BaseRetrySeconds = 30;
    private static readonly string WorkerLeaseOwner = BuildWorkerLeaseOwner();

    private readonly CardDbContext _dbContext;
    private readonly OperationExecutor _operationExecutor;
    private readonly IAlertService _alertService;
    private readonly IAuditStampService _auditStampService;
    private readonly ReconciliationOptions _options = new();

    public ExecuteService(
        CardDbContext dbContext,
        OperationExecutor operationExecutor,
        IAlertService alertService,
        IAuditStampService auditStampService,
        IOptions<ReconciliationOptions> options)
    {
        _dbContext = dbContext;
        _operationExecutor = operationExecutor;
        _alertService = alertService;
        _auditStampService = auditStampService;
        _options = options.Value;
    }

    public async Task<ExecuteResponse> ExecuteAsync(
        ExecuteRequest request,
        List<ReconciliationErrorDetail>? errors = null,
        CancellationToken cancellationToken = default)
    {
        errors ??= new List<ReconciliationErrorDetail>();
        var now = DateTime.Now;
        var executeOptions = ResolveExecuteOptions(request);

        var response = new ExecuteResponse
        {
            Errors = errors
        };

        var remaining = executeOptions.MaxEvaluations;

        var evaluationIds = await ResolveTargetEvaluationIdsAsync(
            request,
            now,
            executeOptions.MaxEvaluations,
            cancellationToken);

        foreach (var evaluationId in evaluationIds)
        {
            if (remaining <= 0)
            {
                break;
            }

            var results = await ExecuteEvaluationAsync(
                evaluationId,
                now,
                remaining,
                executeOptions.LeaseSeconds,
                errors,
                cancellationToken);

            foreach (var result in results)
            {
                response.Results.Add(result);
                UpdateExecutionTotals(response, result);
            }

            remaining = executeOptions.MaxEvaluations - response.TotalAttempted;
        }

        try
        {
            await _alertService.ExecuteAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            errors.Add(ReconciliationErrorMapper.MapException(ex, "RECONCILIATION_ALERT_SERVICE_EXECUTE"));
        }
        response.ErrorCount = errors.Count;
        return response;
    }

    private async Task<Guid[]> ResolveTargetEvaluationIdsAsync(
        ExecuteRequest request,
        DateTime now,
        int maxEvaluations,
        CancellationToken cancellationToken)
    {
        var explicitOperationIds = request.OperationIds?
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToArray() ?? [];

        var explicitGroupIds = request.GroupIds?
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToArray() ?? [];

        var query = _dbContext.ReconciliationOperations
            .AsNoTracking()
            .Where(x =>
                x.Status == OperationStatus.Planned ||
                x.Status == OperationStatus.Blocked ||
                x.Status == OperationStatus.Executing)
            .Where(x => !x.NextAttemptAt.HasValue || x.NextAttemptAt.Value <= now)
            .Where(x => !x.LeaseExpiresAt.HasValue || x.LeaseExpiresAt.Value <= now);

        if (explicitOperationIds.Length > 0)
        {
            query = query.Where(x => explicitOperationIds.Contains(x.Id));
        }

        if (explicitGroupIds.Length > 0)
        {
            query = query.Where(x => explicitGroupIds.Contains(x.GroupId));
        }

        return await query
            .OrderBy(x => x.EvaluationId)
            .ThenBy(x => x.SequenceIndex)
            .Select(x => x.EvaluationId)
            .Distinct()
            .Take(maxEvaluations)
            .ToArrayAsync(cancellationToken);
    }

    private async Task<List<OperationExecutionResult>> ExecuteEvaluationAsync(
        Guid evaluationId,
        DateTime now,
        int remainingLimit,
        int leaseSeconds,
        List<ReconciliationErrorDetail> errors,
        CancellationToken cancellationToken)
    {
        var results = new List<OperationExecutionResult>();

        while (results.Count < remainingLimit)
        {
            _dbContext.ChangeTracker.Clear();

            var operations = await LoadEvaluationOperationsAsync(evaluationId, cancellationToken);
            var nextOperation = GetNextOperation(operations, now);

            if (nextOperation is null)
            {
                break;
            }

            if (!await TryClaimOperationAsync(nextOperation, now, leaseSeconds, cancellationToken))
            {
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

    private async Task<List<ReconciliationOperation>> LoadEvaluationOperationsAsync(
        Guid evaluationId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.ReconciliationOperations
            .AsTracking()
            .Where(x => x.EvaluationId == evaluationId)
            .OrderBy(x => x.SequenceIndex)
            .ToListAsync(cancellationToken);
    }

    private static ReconciliationOperation? GetNextOperation(
        IReadOnlyList<ReconciliationOperation> operations,
        DateTime now)
    {
        foreach (var operation in operations.OrderBy(x => x.SequenceIndex))
        {
            if (operation.Status is OperationStatus.Completed or OperationStatus.Cancelled or OperationStatus.Failed)
            {
                continue;
            }

            if (operation.Status == OperationStatus.Executing)
            {
                if (!HasExpiredLease(operation, now))
                {
                    return null;
                }

                operation.Status = OperationStatus.Planned;
                ReleaseLease(operation);
            }

            if (operation.Status == OperationStatus.Blocked)
            {
                if (!CanPromoteToPlanned(operation, operations))
                {
                    return null;
                }

                operation.Status = OperationStatus.Planned;
            }

            if (!IsReadyForExecution(operation, now))
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
            x.SequenceIndex < operation.SequenceIndex &&
            x.Status != OperationStatus.Completed &&
            x.Status != OperationStatus.Cancelled &&
            x.Status != OperationStatus.Failed);
    }

    private static bool HasExpiredLease(ReconciliationOperation operation, DateTime now)
    {
        return operation.LeaseExpiresAt.HasValue && operation.LeaseExpiresAt.Value <= now;
    }

    private static bool IsReadyForExecution(ReconciliationOperation operation, DateTime now)
    {
        return operation.Status == OperationStatus.Planned &&
               (!operation.NextAttemptAt.HasValue || operation.NextAttemptAt.Value <= now) &&
               (!operation.LeaseExpiresAt.HasValue || operation.LeaseExpiresAt.Value <= now);
    }

    private static bool IsRootOperation(ReconciliationOperation operation)
    {
        return operation.SequenceIndex == 0 && !operation.ParentSequenceIndex.HasValue;
    }

    private static bool HasCompletedParent(
        ReconciliationOperation operation,
        IReadOnlyList<ReconciliationOperation> operations)
    {
        if (!operation.ParentSequenceIndex.HasValue)
        {
            return false;
        }

        var parentOperation = operations.SingleOrDefault(x => x.SequenceIndex == operation.ParentSequenceIndex.Value);
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
        await _dbContext.ReconciliationOperationExecutions.AddAsync(execution, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            if (string.Equals(operation.Code, OperationCodes.CreateManualReview, StringComparison.Ordinal))
            {
                return await ExecuteManualGateAsync(
                    operation,
                    evaluationOperations,
                    execution,
                    errors,
                    cancellationToken);
            }

            var alreadyApplied = await _dbContext.ReconciliationOperations
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
                    ExecutionStatus.Skipped,
                    "SKIPPED_ALREADY_APPLIED",
                    "Operation was already applied for this transaction.",
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

                errors.Add(ReconciliationErrorMapper.Create(
                    "OPERATION_EXECUTION_FAILED",
                    handlerResult.ErrorMessage ??
                    handlerResult.ResultMessage ??
                    $"Operation '{operation.Code}' failed.",
                    "EXECUTION_OPERATION",
                    fileLineId: operation.FileLineId,
                    operationId: operation.Id,
                    evaluationId: operation.EvaluationId,
                    detail: $"Operation code: {operation.Code}, Attempt: {attemptNumber}"));
            }

            execution.Status = handlerResult.IsSkipped
                ? ExecutionStatus.Skipped
                : handlerResult.IsSuccessful
                    ? ExecutionStatus.Completed
                    : ExecutionStatus.Failed;

            execution.FinishedAt = DateTime.Now;
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
            errors.Add(ReconciliationErrorMapper.MapException(
                ex,
                "EXECUTION_OPERATION",
                fileLineId: operation.FileLineId,
                operationId: operation.Id,
                evaluationId: operation.EvaluationId,
                message: $"Unexpected error occurred while executing operation '{operation.Code}'."));

            ScheduleRetry(operation, now, ex.Message);

            execution.Status = ExecutionStatus.Failed;
            execution.FinishedAt = DateTime.Now;
            execution.ResultCode = "FAILED";
            execution.ResultMessage = "Operation execution failed.";
            execution.ErrorCode = "OPERATION_EXECUTION_FAILED";
            execution.ErrorMessage = ex.Message;

            if (operation.Status == OperationStatus.Failed)
            {
                await AddOperationFailureAlertAsync(operation, ex.Message, cancellationToken);
            }

            _auditStampService.StampForUpdate(new AuditEntity[] { operation, execution });
            await _dbContext.SaveChangesAsync(cancellationToken);

            return CreateOperationResult(operation.Id, "Failed", ex.Message);
        }
    }

    private async Task<OperationExecutionResult> ExecuteManualGateAsync(
        ReconciliationOperation operation,
        IReadOnlyList<ReconciliationOperation> evaluationOperations,
        ReconciliationOperationExecution execution,
        List<ReconciliationErrorDetail> errors,
        CancellationToken cancellationToken)
    {
        var review = await _dbContext.ReconciliationReviews
            .AsTracking()
            .SingleOrDefaultAsync(x => x.OperationId == operation.Id, cancellationToken);

        if (review is null)
        {
            errors.Add(ReconciliationErrorMapper.Create(
                "MANUAL_REVIEW_NOT_FOUND",
                "Manual review record could not be resolved.",
                "EXECUTION_MANUAL_GATE",
                fileLineId: operation.FileLineId,
                operationId: operation.Id,
                evaluationId: operation.EvaluationId));

            operation.Status = OperationStatus.Failed;
            ReleaseLease(operation);
            operation.LastError = "Manual review record could not be resolved.";

            execution.Status = ExecutionStatus.Failed;
            execution.FinishedAt = DateTime.Now;
            execution.ResultCode = "MANUAL_REVIEW_NOT_FOUND";
            execution.ResultMessage = "Manual review record could not be resolved.";
            execution.ErrorCode = "MANUAL_REVIEW_NOT_FOUND";
            execution.ErrorMessage = "Manual review record could not be resolved.";

            _auditStampService.StampForUpdate(new AuditEntity[] { operation, execution });
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new OperationExecutionResult
            {
                OperationId = operation.Id,
                Status = "Failed",
                Message = "Manual review record could not be resolved."
            };
        }

        if (review.Decision == ReviewDecision.Pending &&
            review.ExpiresAt.HasValue &&
            review.ExpiresAt.Value <= DateTime.Now)
        {
            ApplyExpirationDecision(review);
        }

        if (review.Decision == ReviewDecision.Pending)
        {
            operation.Status = OperationStatus.Blocked;
            ReleaseLease(operation);
            operation.NextAttemptAt = review.ExpiresAt ?? DateTime.Now.AddSeconds(BaseRetrySeconds);

            execution.Status = ExecutionStatus.Skipped;
            execution.FinishedAt = DateTime.Now;
            execution.ResultCode = "WAITING_MANUAL_DECISION";
            execution.ResultMessage = "Manual review decision is pending.";
            execution.ResponsePayload = JsonSerializer.Serialize(new { review.Decision, review.ExpiresAt });

            _auditStampService.StampForUpdate(new AuditEntity[] { operation, execution });
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new OperationExecutionResult
            {
                OperationId = operation.Id,
                Status = "Blocked",
                Message = "Manual review decision is pending."
            };
        }

        ResolveManualGate(operation, evaluationOperations, review);
        ReleaseLease(operation);
        operation.LastError = null;

        execution.Status = ExecutionStatus.Completed;
        execution.FinishedAt = DateTime.Now;
        execution.ResultCode = $"MANUAL_GATE_{review.Decision.ToString().ToUpperInvariant()}";
        execution.ResultMessage = $"Manual gate resolved with decision '{review.Decision}'.";
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
            .Where(x => x.ParentSequenceIndex == gateOperation.SequenceIndex &&
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
                                 x.SequenceIndex > gateOperation.SequenceIndex &&
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

    private static void ApplyExpirationDecision(ReconciliationReview review)
    {
        review.Decision = review.ExpirationAction switch
        {
            ReviewExpirationAction.Approve => ReviewDecision.Approved,
            ReviewExpirationAction.Reject => ReviewDecision.Rejected,
            _ => ReviewDecision.Cancelled
        };

        review.DecisionAt = DateTime.Now;
    }

    private async Task<OperationExecutionResult> CompleteExecutionAsync(
        ReconciliationOperation operation,
        ReconciliationOperationExecution execution,
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
        execution.FinishedAt = DateTime.Now;
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

    private ExecuteOptions ResolveExecuteOptions(ExecuteRequest request)
    {
        return request.Options ?? _options.Execute;
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
        var alert = new ReconciliationAlert
        {
            Id = Guid.NewGuid(),
            FileLineId = operation.FileLineId,
            GroupId = operation.GroupId, // sadece informatik amaçlı tutuluyor
            EvaluationId = operation.EvaluationId,
            OperationId = operation.Id,
            Severity = "Error",
            AlertType = "OperationExecutionFailed",
            Message = message
        };
        _auditStampService.StampForCreate(alert);
        await _dbContext.ReconciliationAlerts.AddAsync(alert, cancellationToken);
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
}

using System.Text.Json;
using LinkPara.Card.Application.Commons.Helpers.Reconciliation;
using LinkPara.Card.Application.Commons.Models.Reconciliation;
using LinkPara.Card.Domain.Entities.Reconciliation;
using LinkPara.Card.Domain.Enums.Reconciliation;
using LinkPara.Card.Infrastructure.Persistence;
using LinkPara.Card.Infrastructure.Services.Audit;
using LinkPara.Card.Infrastructure.Services.Reconciliation.Execute;
using LinkPara.SharedModels.Persistence;
using LinkPara.Card.Infrastructure.Services.Reconciliation.Evaluate;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Reviews;

internal sealed class ReviewService
{
    private readonly CardDbContext _dbContext;
    private readonly IAuditStampService _auditStampService;

    public ReviewService(CardDbContext dbContext, IAuditStampService auditStampService)
    {
        _dbContext = dbContext;
        _auditStampService = auditStampService;
    }

    public async Task<ApproveResponse> ApproveAsync(
        ApproveRequest request,
        List<ReconciliationErrorDetail>? errors = null,
        CancellationToken cancellationToken = default)
    {
        errors ??= new List<ReconciliationErrorDetail>();
        try
        {
            var (result, message) = await SetDecisionAsync(
                request.OperationId,
                request.ReviewerId,
                ReviewDecision.Approved,
                request.Comment,
                errors,
                cancellationToken);

            return new ApproveResponse
            {
                OperationId = request.OperationId,
                Result = result,
                Message = message,
                Errors = errors,
                ErrorCount = errors.Count
            };
        }
        catch (Exception ex)
        {
            errors.Add(ReconciliationErrorMapper.MapException(
                ex,
                "REVIEW_APPROVE",
                operationId: request.OperationId,
                message: "Manual review approval could not be completed."));

            return new ApproveResponse
            {
                OperationId = request.OperationId,
                Result = "Failed",
                Message = "Manual review approval failed. See errors for details.",
                Errors = errors,
                ErrorCount = errors.Count
            };
        }
    }

    public async Task<RejectResponse> RejectAsync(
        RejectRequest request,
        List<ReconciliationErrorDetail>? errors = null,
        CancellationToken cancellationToken = default)
    {
        errors ??= new List<ReconciliationErrorDetail>();
        try
        {
            var (result, message) = await SetDecisionAsync(
                request.OperationId,
                request.ReviewerId,
                ReviewDecision.Rejected,
                request.Comment,
                errors,
                cancellationToken);

            return new RejectResponse
            {
                OperationId = request.OperationId,
                Result = result,
                Message = message,
                Errors = errors,
                ErrorCount = errors.Count
            };
        }
        catch (Exception ex)
        {
            errors.Add(ReconciliationErrorMapper.MapException(
                ex,
                "REVIEW_REJECT",
                operationId: request.OperationId,
                message: "Manual review rejection could not be completed."));

            return new RejectResponse
            {
                OperationId = request.OperationId,
                Result = "Failed",
                Message = "Manual review rejection failed. See errors for details.",
                Errors = errors,
                ErrorCount = errors.Count
            };
        }
    }

    public async Task<PendingReviewsResponse> GetPendingAsync(
        PendingReviewsRequest request,
        List<ReconciliationErrorDetail>? errors = null,
        CancellationToken cancellationToken = default)
    {
        errors ??= new List<ReconciliationErrorDetail>();
        try
        {
            var page = Math.Max(request.Page, 1);
            var pageSize = Math.Clamp(request.PageSize, 1, 1000);
            var skip = (page - 1) * pageSize;

            var query = _dbContext.ReconciliationReviews
                .AsNoTracking()
                .Where(x => x.Decision == ReviewDecision.Pending)
                .AsQueryable();

            if (request.Date.HasValue)
            {
                var start = request.Date.Value.ToDateTime(TimeOnly.MinValue);
                var end = start.AddDays(1);

                query = query.Where(x => x.CreateDate >= start && x.CreateDate < end);
            }

            var joinedQuery = query
                .Join(
                    _dbContext.ReconciliationOperations.AsNoTracking(),
                    review => review.OperationId,
                    operation => operation.Id,
                    (review, operation) => new { review, operation })
                .OrderBy(x => x.review.CreateDate);

            var total = await joinedQuery.CountAsync(cancellationToken);

            var reviewWithOps = await joinedQuery
                .Skip(skip)
                .Take(pageSize)
                .Select(x => new
                {
                    Review = x.review,
                    Operation = x.operation
                })
                .ToListAsync(cancellationToken);

            var evaluationIds = reviewWithOps
                .Select(x => x.Operation.EvaluationId)
                .Distinct()
                .ToArray();

            var branchOpsByEvaluation = await _dbContext.ReconciliationOperations
                .AsNoTracking()
                .Where(o => evaluationIds.Contains(o.EvaluationId))
                .ToListAsync(cancellationToken);

            var items = new List<ManualReview>();

            foreach (var entry in reviewWithOps)
            {
                var review = entry.Review;
                var op = entry.Operation;

                var manualReview = new ManualReview
                {
                    OperationId = op.Id,
                    FileLineId = op.FileLineId,
                    OperationCode = op.Code,
                    OperationPayload = op.Payload,
                    CreatedAt = review.CreateDate,
                    ExpiresAt = review.ExpiresAt,
                    ExpirationAction = review.ExpirationAction.ToString(),
                    ExpirationFlowAction = review.ExpirationFlowAction.ToString(),
                    ApprovalMessage = "If approved, the transaction will follow the 'Approve' flow.",
                    RejectionMessage = "If rejected, the transaction will follow the 'Reject' flow."
                };
                
                var branchOps = branchOpsByEvaluation
                    .Where(o => o.EvaluationId == op.EvaluationId && o.ParentSequenceIndex == op.SequenceIndex)
                    .ToList();

                foreach (var b in branchOps)
                {
                    var branchOperation = new BranchOperation
                    {
                        Code = b.Code,
                        Payload = b.Payload
                    };

                    if (string.Equals(b.Branch, Branches.Approve, StringComparison.OrdinalIgnoreCase))
                    {
                        manualReview.ApproveBranchOperations.Add(branchOperation);
                    }
                    else if (string.Equals(b.Branch, Branches.Reject, StringComparison.OrdinalIgnoreCase))
                    {
                        manualReview.RejectBranchOperations.Add(branchOperation);
                    }
                }

                if (string.Equals(op.Code, OperationCodes.CreateManualReview, StringComparison.Ordinal))
                {
                    manualReview.OperationPayload = BuildManualReviewPayload(manualReview);
                }

                items.Add(manualReview);
            }

            return new PendingReviewsResponse
            {
                Page = new PagedResult<ManualReview>
                {
                    Page = page,
                    PageSize = pageSize,
                    Total = total,
                    Items = items
                },
                Errors = errors,
                ErrorCount = errors.Count
            };
        }
        catch (Exception ex)
        {
            errors.Add(ReconciliationErrorMapper.MapException(
                ex,
                "GET_PENDING_REVIEWS_QUERY",
                message: "Pending reconciliation reviews could not be loaded."));

            return new PendingReviewsResponse
            {
                Errors = errors,
                ErrorCount = errors.Count
            };
        }
    }

    private async Task<(string Result, string Message)> SetDecisionAsync(
        Guid operationId,
        Guid? reviewerId,
        ReviewDecision decision,
        string? comment,
        List<ReconciliationErrorDetail>? errors,
        CancellationToken cancellationToken)
    {
        try
        {
            var review = await _dbContext.ReconciliationReviews
                .AsTracking()
                .SingleOrDefaultAsync(x => x.OperationId == operationId, cancellationToken);
            var operation = await _dbContext.ReconciliationOperations
                .AsTracking()
                .SingleOrDefaultAsync(x => x.Id == operationId, cancellationToken);

            if (review is null)
            {
                errors?.Add(ReconciliationErrorMapper.Create(
                    "REVIEW_NOT_FOUND",
                    "Review record was not found for the manual operation.",
                    "REVIEW_DECISION",
                    operationId: operationId));
                return ("NotFound", "Review record was not found for the manual operation.");
            }

            if (review.Decision != ReviewDecision.Pending)
            {
                errors?.Add(ReconciliationErrorMapper.Create(
                    "REVIEW_ALREADY_FINALIZED",
                    "Review decision is already finalized.",
                    "REVIEW_DECISION",
                    operationId: operationId));
                return ("Invalid", "Review decision is already finalized.");
            }

            var currentReviewerId = ResolveReviewerId(reviewerId);
            review.ReviewerId = currentReviewerId;
            review.Decision = decision;
            review.Comment = comment;
            review.DecisionAt = DateTime.Now;
            if (operation is not null &&
                operation.Status != OperationStatus.Completed &&
                operation.Status != OperationStatus.Cancelled &&
                operation.Status != OperationStatus.Failed)
            {
                operation.NextAttemptAt = DateTime.Now;
                operation.LeaseExpiresAt = null;
                operation.LeaseOwner = null;
            }

            var entitiesToUpdate = operation is null
                ? new AuditEntity[] { review }
                : new AuditEntity[] { review, operation };
            _auditStampService.StampForUpdate(entitiesToUpdate);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var result = decision == ReviewDecision.Approved ? "Approved" : "Rejected";
            return (result, $"Manual review {result.ToLowerInvariant()}.");
        }
        catch (Exception ex)
        {
            errors?.Add(ReconciliationErrorMapper.MapException(
                ex,
                "REVIEW_DECISION",
                operationId: operationId,
                message: "Manual review decision could not be persisted."));
            return ("Failed", "Manual review decision failed. See errors for details.");
        }
    }

    private Guid ResolveReviewerId(Guid? requestedReviewerId)
    {
        if (requestedReviewerId.HasValue && requestedReviewerId.Value != Guid.Empty)
        {
            return requestedReviewerId.Value;
        }

        var auditStamp = _auditStampService.CreateStamp();
        if (Guid.TryParse(auditStamp.UserId, out var currentReviewerId))
        {
            return currentReviewerId;
        }

        throw new InvalidOperationException("Reviewer identifier could not be resolved from the authenticated user or request payload.");
    }

    private static string BuildManualReviewPayload(ManualReview manualReview)
    {
        var payload = new
        {
            approveOperations = string.Join(",",manualReview.ApproveBranchOperations.Select(x=>x.Code)),
            rejectOperations = string.Join(",",manualReview.RejectBranchOperations.Select(x=>x.Code)),
        };

        return JsonSerializer.Serialize(payload);
    }
}

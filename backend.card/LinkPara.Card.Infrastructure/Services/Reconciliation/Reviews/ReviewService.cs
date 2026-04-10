using System.Text.Json;
using LinkPara.Card.Application.Commons.Interfaces;
using Microsoft.Extensions.Localization;
using LinkPara.Card.Application.Commons.Interfaces.Reconciliation;
using LinkPara.Card.Application.Commons.Extensions;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Contracts.Requests;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Contracts.Responses;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Shared;
using LinkPara.Card.Domain.Enums.Reconciliation;
using LinkPara.Card.Infrastructure.Persistence;
using LinkPara.Card.Infrastructure.Services.Audit;
using LinkPara.Card.Infrastructure.Services.Reconciliation.Evaluate.Core;
using LinkPara.Card.Infrastructure.Services.Reconciliation.Execute.Core;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Reviews;

internal sealed class ReviewService
{
    private readonly CardDbContext _dbContext;
    private readonly IAuditStampService _auditStampService;
    private readonly IReconciliationErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ITimeProvider _timeProvider;

    public ReviewService(
        CardDbContext dbContext,
        IAuditStampService auditStampService,
        ITimeProvider timeProvider,
        IReconciliationErrorMapper errorMapper,
        Func<LinkPara.Card.Application.Commons.Localization.LocalizerResource, IStringLocalizer> localizerFactory)
    {
        _dbContext = dbContext;
        _auditStampService = auditStampService;
        _timeProvider = timeProvider;
        _errorMapper = errorMapper;
        _localizer = localizerFactory(LinkPara.Card.Application.Commons.Localization.LocalizerResource.Messages);
    }

    public async Task<ApproveResponse> ApproveAsync(
        ApproveRequest request,
        List<ReconciliationErrorDetail>? errors = null,
        CancellationToken cancellationToken = default)
    {
        errors ??= new List<ReconciliationErrorDetail>();
        if (request is null)
        {
            errors.Add(_errorMapper.Create("INVALID_REQUEST", _localizer.Get("Reconciliation.RequestIsNull"), "REVIEW_APPROVE"));
            return new ApproveResponse
            {
                Result = "Failed",
                Message = _localizer.Get("Reconciliation.RequestFailed"),
                Errors = errors,
                ErrorCount = errors.Count
            };
        }

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
            errors.Add(_errorMapper.MapException(
                ex,
                "REVIEW_APPROVE",
                operationId: request.OperationId,
                message: _localizer.Get("Reconciliation.ManualReviewApprovalCouldNotComplete")));

            return new ApproveResponse
            {
                OperationId = request.OperationId,
                Result = "Failed",
                Message = _localizer.Get("Reconciliation.ManualReviewApprovalFailedSeeErrors"),
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
        if (request is null)
        {
            errors.Add(_errorMapper.Create("INVALID_REQUEST", _localizer.Get("Reconciliation.RequestIsNull"), "REVIEW_REJECT"));
            return new RejectResponse
            {
                Result = "Failed",
                Message = _localizer.Get("Reconciliation.RequestFailed"),
                Errors = errors,
                ErrorCount = errors.Count
            };
        }

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
            errors.Add(_errorMapper.MapException(
                ex,
                "REVIEW_REJECT",
                operationId: request.OperationId,
                message: _localizer.Get("Reconciliation.ManualReviewRejectionCouldNotComplete")));

            return new RejectResponse
            {
                OperationId = request.OperationId,
                Result = "Failed",
                Message = _localizer.Get("Reconciliation.ManualReviewRejectionFailedSeeErrors"),
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
        if (request is null)
        {
            errors.Add(_errorMapper.Create("INVALID_REQUEST", _localizer.Get("Reconciliation.RequestIsNull"), "GET_PENDING_REVIEWS_QUERY"));
            return new PendingReviewsResponse
            {
                Message = _localizer.Get("Reconciliation.RequestFailed"),
                Errors = errors,
                ErrorCount = errors.Count
            };
        }

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

            var branchOpsRaw = await _dbContext.ReconciliationOperations
                .AsNoTracking()
                .Where(o => evaluationIds.Contains(o.EvaluationId) && o.ParentSequenceIndex != null)
                .Select(o => new { o.EvaluationId, o.ParentSequenceIndex, o.Code, o.Payload, o.Branch })
                .ToListAsync(cancellationToken);

            var branchOpsLookup = branchOpsRaw
                .GroupBy(o => (o.EvaluationId, o.ParentSequenceIndex))
                .ToDictionary(g => g.Key, g => g.ToList());

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
                    ApprovalMessage = _localizer.Get("Reconciliation.ApprovalMessage"),
                    RejectionMessage = _localizer.Get("Reconciliation.RejectionMessage")
                };

                var key = (op.EvaluationId, ParentSequenceIndex: (int?)op.SequenceIndex);
                var branchOps = branchOpsLookup.TryGetValue(key, out var matched) ? matched : [];

                foreach (var b in branchOps)
                {
                    var branchOperation = new BranchOperation
                    {
                        Code = b.Code,
                        Payload = b.Payload
                    };

                    if (string.Equals(b.Branch, Branches.Approve, StringComparison.OrdinalIgnoreCase))
                        manualReview.ApproveBranchOperations.Add(branchOperation);
                    else if (string.Equals(b.Branch, Branches.Reject, StringComparison.OrdinalIgnoreCase))
                        manualReview.RejectBranchOperations.Add(branchOperation);
                }

                if (string.Equals(op.Code, OperationCodes.CreateManualReview, StringComparison.Ordinal))
                    manualReview.OperationPayload = BuildManualReviewPayload(manualReview);

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
            errors.Add(_errorMapper.MapException(
                ex,
                "GET_PENDING_REVIEWS_QUERY",
                message: _localizer.Get("Reconciliation.PendingReviewsLoadFailed")));

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
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

                var currentReviewerId = ResolveReviewerId(reviewerId);
                var auditStamp = _auditStampService.CreateStamp();
                var now = _timeProvider.Now;

                var updatedReviewRows = await _dbContext.ReconciliationReviews
                    .Where(x => x.OperationId == operationId && x.Decision == ReviewDecision.Pending)
                    .ExecuteUpdateAsync(update => update
                        .SetProperty(x => x.ReviewerId, currentReviewerId)
                        .SetProperty(x => x.Decision, decision)
                        .SetProperty(x => x.Comment, comment)
                        .SetProperty(x => x.DecisionAt, now)
                        .SetProperty(x => x.UpdateDate, auditStamp.Timestamp)
                        .SetProperty(x => x.LastModifiedBy, auditStamp.UserId),
                        cancellationToken);

                if (updatedReviewRows == 0)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return await BuildMissingOrFinalizedReviewResultAsync(operationId, errors, cancellationToken);
                }

                var updatedOperationRows = await _dbContext.ReconciliationOperations
                    .Where(x => x.Id == operationId)
                    .Where(x =>
                        x.Status != OperationStatus.Completed &&
                        x.Status != OperationStatus.Cancelled &&
                        x.Status != OperationStatus.Failed)
                    .ExecuteUpdateAsync(update => update
                        .SetProperty(x => x.NextAttemptAt, now)
                        .SetProperty(x => x.LeaseExpiresAt, (DateTime?)null)
                        .SetProperty(x => x.LeaseOwner, (string?)null)
                        .SetProperty(x => x.UpdateDate, auditStamp.Timestamp)
                        .SetProperty(x => x.LastModifiedBy, auditStamp.UserId),
                        cancellationToken);

                if (updatedOperationRows == 0)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return await BuildNonRequeueableOperationResultAsync(operationId, errors, cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);

                var result = decision == ReviewDecision.Approved ? "Approved" : "Rejected";
                return (result, _localizer.Get("Reconciliation.ManualReviewResult", result.ToLowerInvariant()));
            });
        }
        catch (Exception ex)
        {
            errors?.Add(_errorMapper.MapException(
                ex,
                "REVIEW_DECISION",
                operationId: operationId,
                message: _localizer.Get("Reconciliation.ManualReviewDecisionPersistFailed")));
            return ("Failed", _localizer.Get("Reconciliation.ManualReviewDecisionFailedSeeErrors"));
        }
    }

    private async Task<(string Result, string Message)> BuildMissingOrFinalizedReviewResultAsync(
        Guid operationId,
        List<ReconciliationErrorDetail>? errors,
        CancellationToken cancellationToken)
    {
        var exists = await _dbContext.ReconciliationReviews
            .AsNoTracking()
            .AnyAsync(x => x.OperationId == operationId, cancellationToken);

        if (!exists)
        {
            var notFoundMsg = _localizer.Get("Reconciliation.ReviewNotFound");
            errors?.Add(_errorMapper.Create(
                "REVIEW_NOT_FOUND",
                notFoundMsg,
                "REVIEW_DECISION",
                operationId: operationId));
            return ("NotFound", notFoundMsg);
        }

        var finalizedMsg = _localizer.Get("Reconciliation.ReviewAlreadyFinalized");
        errors?.Add(_errorMapper.Create(
            "REVIEW_ALREADY_FINALIZED",
            finalizedMsg,
            "REVIEW_DECISION",
            operationId: operationId));
        return ("Invalid", finalizedMsg);
    }

    private async Task<(string Result, string Message)> BuildNonRequeueableOperationResultAsync(
        Guid operationId,
        List<ReconciliationErrorDetail>? errors,
        CancellationToken cancellationToken)
    {
        var operation = await _dbContext.ReconciliationOperations
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == operationId, cancellationToken);

        if (operation is null)
        {
            var notFoundMsg = _localizer.Get("Reconciliation.ReviewOperationNotFound");
            errors?.Add(_errorMapper.Create(
                "REVIEW_OPERATION_NOT_FOUND",
                notFoundMsg,
                "REVIEW_DECISION",
                operationId: operationId));
            return ("NotFound", notFoundMsg);
        }

        var invalidMsg = _localizer.Get("Reconciliation.ReviewOperationNotRequeueable", operation.Status);
        errors?.Add(_errorMapper.Create(
            "REVIEW_OPERATION_NOT_REQUEUEABLE",
            invalidMsg,
            "REVIEW_DECISION",
            operationId: operationId,
            detail: $"Operation status: {operation.Status}"));
        return ("Invalid", invalidMsg);
    }

    private Guid ResolveReviewerId(Guid? requestedReviewerId)
    {
        if (requestedReviewerId.HasValue && requestedReviewerId.Value != Guid.Empty)
        {
            return requestedReviewerId.Value;
        }

        var auditStamp = _auditStampService.CreateStamp();
        return auditStamp.UserGuid;
    }

    private static string BuildManualReviewPayload(ManualReview manualReview)
    {
        var payload = new
        {
            approveOperations = string.Join(",", manualReview.ApproveBranchOperations.Select(x => x.Code)),
            rejectOperations = string.Join(",", manualReview.RejectBranchOperations.Select(x => x.Code)),
        };

        return JsonSerializer.Serialize(payload);
    }
}

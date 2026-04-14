using LinkPara.Card.Application.Commons.Models.Reconciliation.Contracts.Requests;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Contracts.Responses;
using LinkPara.Card.Application.Features.Reconciliation.Queries.GetAlerts;
using LinkPara.Card.Application.Features.Reconciliation.Queries.PendingReviews;

namespace LinkPara.Card.Application.Commons.Interfaces.Reconciliation;

public interface IReconciliationService
{
    Task<EvaluateResponse> EvaluateAsync(EvaluateRequest request, CancellationToken cancellationToken = default);
    Task<ExecuteResponse> ExecuteAsync(ExecuteRequest request, CancellationToken cancellationToken = default);
    Task<ApproveResponse> ApproveAsync(ApproveRequest request, CancellationToken cancellationToken = default);
    Task<RejectResponse> RejectAsync(RejectRequest request, CancellationToken cancellationToken = default);
    Task<GetPendingReviewsResponse> GetPendingReviewsAsync(GetPendingReviewsQuery query, CancellationToken cancellationToken = default);
    Task<GetAlertsResponse> GetAlertsAsync(GetAlertsQuery query, CancellationToken cancellationToken = default);
}

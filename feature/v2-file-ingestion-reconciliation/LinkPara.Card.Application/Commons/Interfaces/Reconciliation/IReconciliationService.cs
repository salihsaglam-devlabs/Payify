using System.Threading;
using System.Threading.Tasks;
using LinkPara.Card.Application.Commons.Models.Reconciliation;

namespace LinkPara.Card.Application.Commons.Interfaces.Reconciliation
{
    public interface IReconciliationService
    {
        Task<EvaluateResponse> EvaluateAsync(EvaluateRequest request, CancellationToken cancellationToken = default);
        Task<ExecuteResponse> ExecuteAsync(ExecuteRequest request, CancellationToken cancellationToken = default);
        Task<ApproveResponse> ApproveAsync(ApproveRequest request, CancellationToken cancellationToken = default);
        Task<RejectResponse> RejectAsync(RejectRequest request, CancellationToken cancellationToken = default);
        Task<PendingReviewsResponse> GetPendingReviewsAsync(PendingReviewsRequest request, CancellationToken cancellationToken = default);
        Task<GetAlertsResponse> GetAlertsAsync(GetAlertsRequest request, CancellationToken cancellationToken = default);
    }
}

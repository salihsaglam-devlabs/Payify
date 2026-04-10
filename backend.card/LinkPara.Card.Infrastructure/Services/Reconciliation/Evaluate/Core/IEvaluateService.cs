using LinkPara.Card.Application.Commons.Models.Reconciliation.Contracts.Requests;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Contracts.Responses;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Shared;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Evaluate.Core;

public interface IEvaluateService
{
    Task<EvaluateResponse> EvaluateAsync(
        EvaluateRequest request,
        List<ReconciliationErrorDetail>? errors = null,
        CancellationToken cancellationToken = default);
}

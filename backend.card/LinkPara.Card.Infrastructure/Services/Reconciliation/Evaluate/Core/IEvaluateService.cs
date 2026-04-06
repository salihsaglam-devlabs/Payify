using System.Collections.Generic;
using LinkPara.Card.Application.Commons.Models.Reconciliation;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Evaluate;

public interface IEvaluateService
{
    Task<EvaluateResponse> EvaluateAsync(
        EvaluateRequest request,
        List<ReconciliationErrorDetail>? errors = null,
        CancellationToken cancellationToken = default);
}

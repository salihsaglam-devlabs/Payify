using LinkPara.Card.Domain.Enums.FileIngestion;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Evaluate.Core;

internal interface IEvaluator
{
    bool CanEvaluate(FileContentType fileContentType);

    Task<EvaluationResult> EvaluateAsync(
        EvaluationContext context,
        CancellationToken cancellationToken = default);
}

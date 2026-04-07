using LinkPara.Card.Domain.Enums.FileIngestion;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Evaluate;

internal sealed class VisaEvaluator : IEvaluator
{
    public bool CanEvaluate(FileContentType fileContentType) => fileContentType == FileContentType.Visa;

    public Task<EvaluationResult> EvaluateAsync(
        EvaluationContext context,
        CancellationToken cancellationToken = default)
    {
        var result = new EvaluationResult();
        result.SetNote("Visa evaluator rules are not defined yet. No automatic or manual action was produced.");
        return Task.FromResult(result);
    }
}

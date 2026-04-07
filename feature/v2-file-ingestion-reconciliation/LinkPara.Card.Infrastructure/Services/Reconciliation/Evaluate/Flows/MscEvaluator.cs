using LinkPara.Card.Domain.Enums.FileIngestion;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Evaluate;

internal sealed class MscEvaluator : IEvaluator
{
    public bool CanEvaluate(FileContentType fileContentType) => fileContentType == FileContentType.Msc;

    public Task<EvaluationResult> EvaluateAsync(
        EvaluationContext context,
        CancellationToken cancellationToken = default)
    {
        var result = new EvaluationResult();
        result.SetNote("MSC evaluator rules are not defined yet. No automatic or manual action was produced.");
        return Task.FromResult(result);
    }
}

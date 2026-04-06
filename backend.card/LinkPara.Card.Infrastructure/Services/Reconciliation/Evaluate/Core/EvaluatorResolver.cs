using LinkPara.Card.Domain.Enums.FileIngestion;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Evaluate;

internal sealed class EvaluatorResolver
{
    private readonly IReadOnlyCollection<IEvaluator> _evaluators;

    public EvaluatorResolver(IEnumerable<IEvaluator> evaluators)
    {
        _evaluators = evaluators.ToArray();
    }

    public IEvaluator Resolve(FileContentType fileContentType)
    {
        var evaluator = _evaluators.FirstOrDefault(x => x.CanEvaluate(fileContentType));
        if (evaluator is null)
        {
            throw new InvalidOperationException($"No reconciliation evaluator registered for '{fileContentType}'.");
        }

        return evaluator;
    }
}

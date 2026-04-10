using LinkPara.Card.Application.Commons.Exceptions;
using LinkPara.Card.Application.Commons.Extensions;
using LinkPara.Card.Domain.Enums.FileIngestion;
using Microsoft.Extensions.Localization;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Evaluate.Core;

internal sealed class EvaluatorResolver
{
    private readonly IReadOnlyCollection<IEvaluator> _evaluators;
    private readonly IStringLocalizer _localizer;

    public EvaluatorResolver(IEnumerable<IEvaluator> evaluators, Func<LinkPara.Card.Application.Commons.Localization.LocalizerResource, IStringLocalizer> localizerFactory)
    {
        _evaluators = evaluators.ToArray();
        _localizer = localizerFactory(LinkPara.Card.Application.Commons.Localization.LocalizerResource.Messages);
    }

    public IEvaluator Resolve(FileContentType fileContentType)
    {
        var evaluator = _evaluators.FirstOrDefault(x => x.CanEvaluate(fileContentType));
        if (evaluator is null)
        {
            throw new ReconciliationConfigurationException(ApiErrorCode.ReconciliationNoEvaluatorRegistered, _localizer.Get("Reconciliation.NoEvaluatorRegistered", fileContentType));
        }

        return evaluator;
    }
}

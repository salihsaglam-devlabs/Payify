using LinkPara.Card.Application.Commons.Extensions;
using LinkPara.Card.Domain.Enums.FileIngestion;
using LinkPara.Card.Infrastructure.Services.Reconciliation.Evaluate.Core;
using Microsoft.Extensions.Localization;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Evaluate.Flows;

internal sealed class VisaEvaluator : IEvaluator
{
    private readonly IStringLocalizer _localizer;

    public VisaEvaluator(Func<LinkPara.Card.Application.Commons.Localization.LocalizerResource, IStringLocalizer> localizerFactory)
    {
        _localizer = localizerFactory(LinkPara.Card.Application.Commons.Localization.LocalizerResource.Messages);
    }

    public bool CanEvaluate(FileContentType fileContentType) => fileContentType == FileContentType.Visa;

    public Task<EvaluationResult> EvaluateAsync(
        EvaluationContext context,
        CancellationToken cancellationToken = default)
    {
        var result = new EvaluationResult();
        result.SetNote(_localizer.Get("Reconciliation.Visa.RulesNotDefined"));
        return Task.FromResult(result);
    }
}

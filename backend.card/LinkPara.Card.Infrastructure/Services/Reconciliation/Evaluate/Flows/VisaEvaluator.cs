using Microsoft.Extensions.Localization;
using LinkPara.Card.Domain.Enums.FileIngestion;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Evaluate;

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

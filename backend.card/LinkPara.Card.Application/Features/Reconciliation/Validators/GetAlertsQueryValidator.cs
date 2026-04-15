using FluentValidation;
using LinkPara.Card.Application.Features.Reconciliation.Queries.GetAlerts;
using Microsoft.Extensions.Localization;

namespace LinkPara.Card.Application.Features.Reconciliation.Validators;

public class GetAlertsQueryValidator : AbstractValidator<GetAlertsQuery>
{
    public GetAlertsQueryValidator(IStringLocalizerFactory factory)
    {
        var localizer = factory.Create("Messages", "LinkPara.Card.API");

        RuleFor(x => x.AlertStatus)
            .IsInEnum()
            .When(x => x.AlertStatus.HasValue)
            .WithMessage(localizer.GetString("Validation.AlertStatusInvalid").Value);

        RuleFor(x => x.Date)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .When(x => x.Date.HasValue)
            .WithMessage(localizer.GetString("Validation.DateCannotBeFuture").Value);
    }
}


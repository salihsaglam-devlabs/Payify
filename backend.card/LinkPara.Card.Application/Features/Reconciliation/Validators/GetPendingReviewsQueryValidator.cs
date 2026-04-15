using FluentValidation;
using LinkPara.Card.Application.Features.Reconciliation.Queries.PendingReviews;
using Microsoft.Extensions.Localization;

namespace LinkPara.Card.Application.Features.Reconciliation.Validators;

public class GetPendingReviewsQueryValidator : AbstractValidator<GetPendingReviewsQuery>
{
    public GetPendingReviewsQueryValidator(IStringLocalizerFactory factory)
    {
        var localizer = factory.Create("Messages", "LinkPara.Card.API");

        RuleFor(x => x.Date)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .When(x => x.Date.HasValue)
            .WithMessage(localizer.GetString("Validation.DateCannotBeFuture").Value);
    }
}


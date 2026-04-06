using FluentValidation;

namespace LinkPara.Emoney.Application.Features.Limits.Queries.GetUserCurrentLimits;

public class GetUserLimitsQueryValidator : AbstractValidator<GetUserLimitsQuery>
{
    public GetUserLimitsQueryValidator()
    {
        RuleFor(s => s.CurrencyCode)
            .NotNull().NotEmpty().MaximumLength(3);

        RuleFor(s => s.UserId)
            .NotNull().NotEmpty();
    }
}
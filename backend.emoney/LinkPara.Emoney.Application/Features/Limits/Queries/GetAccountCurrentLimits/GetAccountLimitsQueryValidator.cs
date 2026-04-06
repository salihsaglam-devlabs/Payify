using FluentValidation;

namespace LinkPara.Emoney.Application.Features.Limits.Queries.GetAccountCurrentLimits;

public class GetAccountLimitsQueryValidator : AbstractValidator<GetAccountLimitsQuery>
{
    public GetAccountLimitsQueryValidator()
    {
        RuleFor(s => s.CurrencyCode)
            .NotNull().NotEmpty().MaximumLength(3);

        RuleFor(s => s.AccountId)
            .NotNull().NotEmpty();
    }
}
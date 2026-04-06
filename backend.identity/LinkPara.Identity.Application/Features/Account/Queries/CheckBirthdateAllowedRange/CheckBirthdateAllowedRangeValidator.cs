using FluentValidation;

namespace LinkPara.Identity.Application.Features.Account.Queries.CheckBirthdateAllowedRange;

public class CheckBirthdateAllowedRangeValidator : AbstractValidator<CheckBirthdateAllowedRangeQuery>
{
    public CheckBirthdateAllowedRangeValidator()
    {
        RuleFor(q => q.BirthDate).NotEqual(DateTime.MinValue);
    }
}

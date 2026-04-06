using FluentValidation;

namespace LinkPara.Calendar.Application.Features.Days.Queries.IsHoliday;

public class IsHolidayQueryValidator : AbstractValidator<IsHolidayQuery>
{
    public IsHolidayQueryValidator()
    {
        RuleFor(s => s.CountryCode)
            .NotEmpty()
            .NotNull()
            .Length(3);

        RuleFor(s => s.Date)
            .NotNull()
            .NotEmpty();
    }
}

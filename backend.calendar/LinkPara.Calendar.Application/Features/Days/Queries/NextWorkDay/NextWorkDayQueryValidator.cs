using FluentValidation;
using LinkPara.Calendar.Application.Features.Days.Queries.NextWorkDay;

namespace LinkPara.Calendar.Application.Features.Days.Queries.NextWorkDay;

public class NextWorkDayQueryValidator : AbstractValidator<NextWorkDayQuery>
{
    public NextWorkDayQueryValidator()
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
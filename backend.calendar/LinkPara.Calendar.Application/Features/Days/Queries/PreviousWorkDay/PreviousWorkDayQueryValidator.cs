using FluentValidation;

namespace LinkPara.Calendar.Application.Features.Days.Queries.PreviousWorkDay;

public class PreviousWorkDayQueryValidator : AbstractValidator<PreviousWorkDayQuery>
{
    public PreviousWorkDayQueryValidator()
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
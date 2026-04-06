using FluentValidation;

namespace LinkPara.Calendar.Application.Features.HolidayDetails.Commands.SaveHolidayDetail;

public class SaveHolidayDetailCommandValidator : AbstractValidator<SaveHolidayDetailCommand>
{
    public SaveHolidayDetailCommandValidator()
    {
        RuleFor(s => s.DurationInDays)
            .NotNull()
            .NotEmpty()
            .GreaterThan(0);

        RuleFor(s => s.Recurrence)
            .NotNull()
            .NotEmpty()
            .GreaterThan(0);

        RuleFor(s => s.HolidayId)
            .NotNull()
            .NotEmpty();

        RuleFor(s => s.BeginningTime)
            .NotNull()
            .NotEmpty();

        RuleFor(s => s.EndingTime)
            .NotNull()
            .NotEmpty();

        RuleFor(s => s.DateOfHoliday)
            .NotNull()
            .NotEmpty();
    }
}

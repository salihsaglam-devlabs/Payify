using FluentValidation;

namespace LinkPara.Calendar.Application.Features.Holidays.Commands.UpdateHoliday;

public class UpdateHolidayCommandValidator : AbstractValidator<UpdateHolidayCommand>
{
    public UpdateHolidayCommandValidator()
    {
        RuleFor(s => s.CountryCode)
               .NotEmpty()
               .NotNull()
               .Length(3);

        RuleFor(s => s.Name)
            .NotNull()
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(s => s.HolidayType)
            .IsInEnum();
    }
}

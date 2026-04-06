using FluentValidation;
using LinkPara.Calendar.Domain.Enums;

namespace LinkPara.Calendar.Application.Features.Holidays.Commands.SaveHoliday;

public class SaveHolidayCommandValidator : AbstractValidator<SaveHolidayCommand>
{
    public SaveHolidayCommandValidator()
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

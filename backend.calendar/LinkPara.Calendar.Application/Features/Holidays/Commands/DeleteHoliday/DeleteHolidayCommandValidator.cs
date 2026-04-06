using FluentValidation;

namespace LinkPara.Calendar.Application.Features.Holidays.Commands.DeleteHoliday;

public class DeleteHolidayCommandValidator : AbstractValidator<DeleteHolidayCommand>
{
    public DeleteHolidayCommandValidator()
    {
        RuleFor(s => s.Id)
            .NotEmpty()
            .NotNull();
    }
}

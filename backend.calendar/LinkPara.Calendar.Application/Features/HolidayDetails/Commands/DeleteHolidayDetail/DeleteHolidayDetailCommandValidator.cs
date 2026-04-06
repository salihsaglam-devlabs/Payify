using FluentValidation;

namespace LinkPara.Calendar.Application.Features.HolidayDetails.Commands.DeleteHolidayDetail;

public class DeleteHolidayDetailCommandValidator : AbstractValidator<DeleteHolidayDetailCommand>
{
    public DeleteHolidayDetailCommandValidator()
    {
        RuleFor(s => s.Id)
        .NotEmpty()
        .NotNull();
    }
}

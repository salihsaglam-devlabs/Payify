using FluentValidation;

namespace LinkPara.Billing.Application.Features.SavedBills.Commands.DeleteSavedBill;

public class DeleteSavedBillCommandValidator : AbstractValidator<DeleteSavedBillCommand>
{
    public DeleteSavedBillCommandValidator()
    {
        RuleFor(s => s.Id).NotEmpty().NotNull();
    }
}
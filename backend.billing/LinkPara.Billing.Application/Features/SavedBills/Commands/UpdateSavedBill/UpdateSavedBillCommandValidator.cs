using FluentValidation;

namespace LinkPara.Billing.Application.Features.SavedBills.Commands.UpdateSavedBill;

public class UpdateSavedBillCommandValidator : AbstractValidator<UpdateSavedBillCommand>
{
    public UpdateSavedBillCommandValidator()
    {
        RuleFor(s => s.Id).NotEmpty().NotNull();
        RuleFor(s => s.BillName).NotEmpty().NotNull();
    }
}
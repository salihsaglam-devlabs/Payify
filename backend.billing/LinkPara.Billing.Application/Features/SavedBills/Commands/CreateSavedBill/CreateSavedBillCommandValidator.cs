using FluentValidation;

namespace LinkPara.Billing.Application.Features.SavedBills.Commands.CreateSavedBill;

public class CreateSavedBillCommandValidator : AbstractValidator<CreateSavedBillCommand>
{
    public CreateSavedBillCommandValidator()
    {
        RuleFor(s => s.InstitutionId).NotEmpty().NotNull();
        RuleFor(s => s.SubscriberNumber1).NotEmpty().NotNull();
        RuleFor(s => s.BillName).NotEmpty().NotNull();
    }
}
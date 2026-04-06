using FluentValidation;

namespace LinkPara.Billing.Application.Features.Billing.Commands.CancelBillAccounting;

public class CancelBillAccountingCommandValidator : AbstractValidator<CancelBillAccountingCommand>
{
    public CancelBillAccountingCommandValidator()
    {
        RuleFor(r => r.TransactionId).NotEmpty().NotNull().NotEqual(Guid.Empty);
    }
}
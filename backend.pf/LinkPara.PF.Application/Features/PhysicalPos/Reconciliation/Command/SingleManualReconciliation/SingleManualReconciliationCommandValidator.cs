using FluentValidation;

namespace LinkPara.PF.Application.Features.PhysicalPos.Reconciliation.Command.SingleManualReconciliation;

public class SingleManualReconciliationCommandValidator : AbstractValidator<SingleManualReconciliationCommand>
{
    public SingleManualReconciliationCommandValidator()
    {
        RuleFor(x => x.MerchantTransactionId).NotNull().NotEmpty();
    }
}

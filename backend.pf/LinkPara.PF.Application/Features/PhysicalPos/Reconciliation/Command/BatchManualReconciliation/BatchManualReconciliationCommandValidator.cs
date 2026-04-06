using FluentValidation;

namespace LinkPara.PF.Application.Features.PhysicalPos.Reconciliation.Command.BatchManualReconciliation;

public class BatchManualReconciliationCommandValidator : AbstractValidator<BatchManualReconciliationCommand>
{
    public BatchManualReconciliationCommandValidator()
    {
        RuleFor(x => x.EndOfDayId).NotNull().NotEmpty();
    }
}

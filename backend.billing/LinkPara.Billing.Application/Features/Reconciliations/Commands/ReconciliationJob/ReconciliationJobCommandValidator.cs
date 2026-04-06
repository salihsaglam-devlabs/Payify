using FluentValidation;

namespace LinkPara.Billing.Application.Features.Reconciliations.Commands.ReconciliationJob;

public class ReconciliationJobCommandValidator : AbstractValidator<ReconciliationJobCommand>
{
    public ReconciliationJobCommandValidator()
    {
        RuleFor(r => r.VendorId).NotEmpty().NotNull().NotEqual(Guid.Empty);
        RuleFor(r => r.ReconciliationDate).NotEmpty().NotNull().NotEqual(DateTime.MinValue);
    }
}

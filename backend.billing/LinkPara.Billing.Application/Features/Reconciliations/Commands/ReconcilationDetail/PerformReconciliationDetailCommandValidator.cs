using FluentValidation;

namespace LinkPara.Billing.Application.Features.Reconciliations.Commands.ReconcilationDetail;

public class GetReconciliationDetailQueryValidator : AbstractValidator<PerformReconciliationDetailCommand>
{
    public GetReconciliationDetailQueryValidator()
    {
        RuleFor(s => s.ReconciliationDate).NotEmpty().NotNull().NotEqual(DateTime.MinValue);
        RuleFor(s => s.VendorId).NotEmpty().NotNull().NotEqual(Guid.Empty);
    }
}
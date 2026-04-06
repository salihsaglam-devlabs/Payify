using FluentValidation;

namespace LinkPara.Billing.Application.Features.Reconciliations.Commands.ReconcilationSummary;

public class PerformReconcilationSummaryCommandValidator : AbstractValidator<PerformReconcilationSummaryCommand>
{
    public PerformReconcilationSummaryCommandValidator()
    {
        RuleFor(q => q.ReconciliationDate).NotEmpty().NotNull().NotEqual(DateTime.MinValue);
        RuleFor(q => q.VendorId).NotEmpty().NotNull().NotEqual(Guid.Empty);
    }
}

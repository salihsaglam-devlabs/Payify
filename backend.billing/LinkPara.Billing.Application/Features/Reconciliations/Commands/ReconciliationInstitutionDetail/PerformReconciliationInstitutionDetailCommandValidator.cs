using FluentValidation;

namespace LinkPara.Billing.Application.Features.Reconciliations.Commands.ReconciliationInstitutionDetail;

public class PerformReconciliationInstitutionDetailCommandValidator : AbstractValidator<PerformReconciliationInstitutionDetailCommand>
{
    public PerformReconciliationInstitutionDetailCommandValidator()
    {
        RuleFor(v => v.InstitutionSummaryId).NotEmpty().NotNull().NotEqual(Guid.Empty);
    }
}
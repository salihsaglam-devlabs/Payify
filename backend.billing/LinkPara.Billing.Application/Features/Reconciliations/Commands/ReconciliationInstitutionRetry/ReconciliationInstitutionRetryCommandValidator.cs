using FluentValidation;

namespace LinkPara.Billing.Application.Features.Reconciliations.Commands.ReconciliationInstitutionRetry;

public class ReconciliationInstitutionRetryCommandValidator : AbstractValidator<ReconciliationInstitutionRetryCommand>
{
    public ReconciliationInstitutionRetryCommandValidator()
    {
        RuleFor(c => c.InstitutionSummaryId).NotEmpty().NotEqual(Guid.Empty);
    }
}
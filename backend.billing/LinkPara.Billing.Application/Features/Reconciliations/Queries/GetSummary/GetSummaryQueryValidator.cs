using FluentValidation;

namespace LinkPara.Billing.Application.Features.Reconciliations.Queries.GetSummary;

public class GetSummaryQueryValidator : AbstractValidator<GetSummaryQuery>
{
    public GetSummaryQueryValidator()
    {
        RuleFor(r => r.VendorId).NotEqual(Guid.Empty);
        RuleFor(r => r.ReconciliationStartDate).NotEmpty().NotNull().NotEqual(DateTime.MinValue);
        RuleFor(r => r.ReconciliationEndDate).NotEmpty().NotNull().NotEqual(DateTime.MinValue);
    }
}
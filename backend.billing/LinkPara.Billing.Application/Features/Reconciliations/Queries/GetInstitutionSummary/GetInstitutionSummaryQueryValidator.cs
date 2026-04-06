using FluentValidation;

namespace LinkPara.Billing.Application.Features.Reconciliations.Queries.GetInstitutionSummary;

public class GetInstitutionSummaryQueryValidator : AbstractValidator<GetInstitutionSummaryQuery>
{
    public GetInstitutionSummaryQueryValidator ()
    {
        RuleFor(q => q.StartTime).NotEqual(DateTime.MinValue);
        RuleFor(q => q.EndTime).NotEqual(DateTime.MinValue);
        RuleFor(q => q.InstitutionId).NotEqual(Guid.Empty);
        RuleFor(q => q.VendorId).NotEqual(Guid.Empty);
    }
}
using FluentValidation;

namespace LinkPara.Billing.Application.Features.Reconciliations.Queries.GetInstitutionDetail;

public class GetInstitutionDetailQueryValidator : AbstractValidator<GetInstitutionDetailQuery>
{
    public GetInstitutionDetailQueryValidator()
    {
        RuleFor(q => q.InstitutionSummaryId).NotEmpty().NotNull().NotEqual(Guid.Empty);
    }
}
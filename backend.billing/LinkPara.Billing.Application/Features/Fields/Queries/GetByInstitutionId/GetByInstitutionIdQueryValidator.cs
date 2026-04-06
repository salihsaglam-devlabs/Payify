using FluentValidation;

namespace LinkPara.Billing.Application.Features.Fields.Queries.GetByInstitutionId;

public class GetByInstitutionIdQueryValidator : AbstractValidator<GetByInstitutionIdQuery>
{
    public GetByInstitutionIdQueryValidator()
    {
        RuleFor(q => q.InstitutionId).NotEmpty().NotNull();
    }
}
using FluentValidation;

namespace LinkPara.Billing.Application.Features.Institutions.Queries.GetById;

public class GetByIdQueryValidator : AbstractValidator<GetByIdQuery>
{
    public GetByIdQueryValidator()
    {
        RuleFor(s => s.InstitutionId).NotNull().NotEmpty();
    }
}
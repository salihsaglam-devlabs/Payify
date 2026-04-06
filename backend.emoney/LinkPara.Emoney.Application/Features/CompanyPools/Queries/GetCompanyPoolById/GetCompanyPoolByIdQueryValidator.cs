using FluentValidation;

namespace LinkPara.Emoney.Application.Features.CompanyPools.Queries.GetCompanyPoolById;

public class GetCompanyPoolByIdQueryValidator : AbstractValidator<GetCompanyPoolByIdQuery>
{
    public GetCompanyPoolByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .NotNull();
    }

}

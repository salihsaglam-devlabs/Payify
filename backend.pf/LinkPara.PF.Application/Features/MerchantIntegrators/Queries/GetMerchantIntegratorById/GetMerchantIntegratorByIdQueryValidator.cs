using FluentValidation;

namespace LinkPara.PF.Application.Features.MerchantIntegrators.Queries.GetMerchantIntegratorById;

public class GetMerchantIntegratorByIdQueryValidator : AbstractValidator<GetMerchantIntegratorByIdQuery>
{
    public GetMerchantIntegratorByIdQueryValidator()
    {
        RuleFor(x => x.Id)
     .NotNull().NotEmpty();
    }
}

using FluentValidation;

namespace LinkPara.PF.Application.Features.MerchantUsers.Queries.GetMerchantUserById;

public class GetMerchantUserByIdQueryValidator : AbstractValidator<GetMerchantUserByIdQuery>
{
    public GetMerchantUserByIdQueryValidator()
    {
        RuleFor(x => x.Id)
       .NotNull().NotEmpty();
    }
}

using FluentValidation;

namespace LinkPara.PF.Application.Features.SubMerchantUsers.Queries.GetSubMerchantUserById;

public class GetSubMerchantUserByIdQueryValidator : AbstractValidator<GetSubMerchantUserByIdQuery>
{
    public GetSubMerchantUserByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotNull().NotEmpty();
    }
}

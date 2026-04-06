using FluentValidation;

namespace LinkPara.PF.Application.Features.SubMerchants.Queries.GetSubMerchantById;

public class GetSubMerchantByIdQueryValidator : AbstractValidator<GetSubMerchantByIdQuery>
{
    public GetSubMerchantByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotNull().NotEmpty();
    }
}
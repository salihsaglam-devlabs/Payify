using FluentValidation;

namespace LinkPara.PF.Application.Features.MerchantBlockages.Queries.GetMerchantBlockageByMerchantId;

public class GetMerchantBlockageByMerchantIdQueryValidator : AbstractValidator<GetMerchantBlockageByMerchantIdQuery>
{
    public GetMerchantBlockageByMerchantIdQueryValidator()
    {
        RuleFor(s => s.MerchantId).NotNull().NotEmpty();
    }
}
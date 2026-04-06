using FluentValidation;

namespace LinkPara.PF.Application.Features.Boa.Merchants.Queries.GetBoaMerchantByMerchantNumber;

public class GetBoaMerchantByMerchantNumberQueryValidator : AbstractValidator<GetBoaMerchantByMerchantNumberQuery>
{
    public GetBoaMerchantByMerchantNumberQueryValidator()
    {
        RuleFor(s => s.MerchantNumber).NotNull().NotEmpty();
    }
}

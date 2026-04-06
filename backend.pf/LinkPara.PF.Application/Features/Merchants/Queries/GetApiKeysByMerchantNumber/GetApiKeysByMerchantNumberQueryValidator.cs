
using FluentValidation;

namespace LinkPara.PF.Application.Features.Merchants.Queries.GetApiKeysByMerchantNumber;

public class GetApiKeysByMerchantNumberQueryValidator : AbstractValidator<GetApiKeysByMerchantNumberQuery>
{
    public GetApiKeysByMerchantNumberQueryValidator()
    {
        RuleFor(s => s.MerchantNumber).NotNull().NotEmpty();
    }
}

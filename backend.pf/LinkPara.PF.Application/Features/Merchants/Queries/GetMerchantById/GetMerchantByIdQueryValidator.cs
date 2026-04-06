using FluentValidation;

namespace LinkPara.PF.Application.Features.Merchants.Queries.GetMerchantById;

public class GetMerchantByIdQueryValidator : AbstractValidator<GetMerchantByIdQuery>
{
    public GetMerchantByIdQueryValidator()
    {
        RuleFor(s => s.Id).NotNull().NotEmpty();
    }
}

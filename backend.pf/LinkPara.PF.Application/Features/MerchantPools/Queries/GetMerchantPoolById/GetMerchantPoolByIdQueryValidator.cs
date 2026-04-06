using FluentValidation;

namespace LinkPara.PF.Application.Features.MerchantPools.Queries.GetMerchantPoolById;

public class GetMerchantPoolByIdQueryValidator : AbstractValidator<GetMerchantPoolByIdQuery>
{
    public GetMerchantPoolByIdQueryValidator()
    {
        RuleFor(x => x.Id)
      .NotNull().NotEmpty();
    }
}

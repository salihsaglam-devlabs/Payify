using FluentValidation;

namespace LinkPara.PF.Application.Features.MerchantCategoryCodes.Queries.GetMccById;

public class GetMccByIdQueryValidator : AbstractValidator<GetMccByIdQuery>
{
    public GetMccByIdQueryValidator()
    {
        RuleFor(x => x.Id)
      .NotNull().NotEmpty();
    }
}

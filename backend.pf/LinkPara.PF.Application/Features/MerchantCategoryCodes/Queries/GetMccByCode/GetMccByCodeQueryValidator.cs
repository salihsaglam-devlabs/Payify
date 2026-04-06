using FluentValidation;

namespace LinkPara.PF.Application.Features.MerchantCategoryCodes.Queries.GetMccByCode;

public class GetMccByCodeQueryValidator : AbstractValidator<GetMccByCodeQuery>
{
    public GetMccByCodeQueryValidator()
    {
        RuleFor(x => x.Code)
            .NotNull()
            .NotEmpty()
            .Length(4);
    }
}

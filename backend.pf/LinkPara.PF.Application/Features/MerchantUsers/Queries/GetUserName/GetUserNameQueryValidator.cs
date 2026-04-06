using FluentValidation;

namespace LinkPara.PF.Application.Features.MerchantUsers.Queries.GetUserName;

public class GetUserNameQueryValidator : AbstractValidator<GetUserNameQuery>
{
    public GetUserNameQueryValidator()
    {
        RuleFor(x => x.Identifier)
            .NotNull().NotEmpty();
    }
}

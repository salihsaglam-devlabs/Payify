using FluentValidation;

namespace LinkPara.Emoney.Application.Features.Wallets.Queries.GetUserWallets;

public class UserWalletsFilterQueryValidator : AbstractValidator<GetUserWalletsFilterQuery>
{
    public UserWalletsFilterQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotNull()
            .NotEmpty();
    }
}


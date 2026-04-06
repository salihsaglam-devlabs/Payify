using FluentValidation;

namespace LinkPara.Emoney.Application.Features.CorporateWallets.Queries.GetAllUsers;

public class GetCorporateWalletUsersQueryValidator : AbstractValidator<GetCorporateWalletUsersQuery>
{
    public GetCorporateWalletUsersQueryValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty().NotNull();
    }
}

using FluentValidation;

namespace LinkPara.Emoney.Application.Features.Accounts.Queries.GetAccountWalletListQuery;

public class GetAccountWalletListQueryValidator : AbstractValidator<GetAccountWalletListQuery>
{
    public GetAccountWalletListQueryValidator()
    {
        RuleFor(s => s.AccountId)
            .NotNull()
            .NotEmpty();
    }
}
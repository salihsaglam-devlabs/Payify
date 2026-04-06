using FluentValidation;

namespace LinkPara.Emoney.Application.Features.Wallets.Queries.GetWalletBalances;

public class GetWalletBalancesQueryValidator : AbstractValidator<GetWalletBalancesQuery>
{
    public GetWalletBalancesQueryValidator()
    {
        RuleFor(x => x.TransactionDate)
           .NotNull()
           .NotEmpty();
    }
}

using FluentValidation;

namespace LinkPara.Emoney.Application.Features.AccountServiceProviders.Queries.GetWalletBalanceList;

public class GetWalletBalanceListQueryValidator : AbstractValidator<GetWalletBalanceListQuery>
{
    public GetWalletBalanceListQueryValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty().NotNull();
    }
}

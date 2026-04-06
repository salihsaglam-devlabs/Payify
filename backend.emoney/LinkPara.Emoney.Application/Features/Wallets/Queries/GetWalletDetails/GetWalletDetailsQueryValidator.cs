using FluentValidation;

namespace LinkPara.Emoney.Application.Features.Wallets.Queries.GetWalletDetails;

public class GetWalletDetailsQueryValidator : AbstractValidator<GetWalletDetailsQuery>
{
    public GetWalletDetailsQueryValidator()
    {
        RuleFor(x => x.WalletId)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.UserId)
            .NotNull()
            .NotEmpty();
    }
}


using FluentValidation;

namespace LinkPara.Emoney.Application.Features.AccountServiceProviders.Queries.GetWalletList;

public class GetWalletListQueryValidator : AbstractValidator<GetWalletListQuery>
{
    public GetWalletListQueryValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty().NotNull();
    }
}

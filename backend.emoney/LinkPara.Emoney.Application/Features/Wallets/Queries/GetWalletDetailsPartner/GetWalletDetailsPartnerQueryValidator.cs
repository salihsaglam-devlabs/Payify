using FluentValidation;

namespace LinkPara.Emoney.Application.Features.Wallets.Queries.GetWalletDetails;

public class GetWalletDetailsPartnerQueryValidator : AbstractValidator<GetWalletDetailsPartnerQuery>
{
    public GetWalletDetailsPartnerQueryValidator()
    {
        RuleFor(x => x)
             .Must(x =>
                (!string.IsNullOrEmpty(x.Msisdn) && (x.WalletId == null || x.WalletId == Guid.Empty)) ||
                (string.IsNullOrEmpty(x.Msisdn) && x.WalletId != null && x.WalletId != Guid.Empty));

        RuleFor(x => x)
            .Must(x => !string.IsNullOrEmpty(x.Msisdn) || (x.WalletId != null && x.WalletId != Guid.Empty));

        RuleFor(x => x.UserId)
            .NotEmpty(); 
    }
}


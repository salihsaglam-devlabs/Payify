using FluentValidation;

namespace LinkPara.Emoney.Application.Features.Wallets.Commands.UpdateWallet;

public class UpdateWalletCommandValidator : AbstractValidator<UpdateWalletCommand>
{
    public UpdateWalletCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.WalletId)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.FriendlyName)
            .Matches(@"^[A-Za-z0-9\-_ ğüşıöçİĞÜŞÖÇ]+$")
            .MaximumLength(50);
    }
}

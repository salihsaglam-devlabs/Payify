using FluentValidation;

namespace LinkPara.Emoney.Application.Features.Wallets.Commands.SaveWallet;

public class SaveWalletCommandValidator : AbstractValidator<SaveWalletCommand>
{
    public SaveWalletCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.CurrencyCode)
            .NotNull()
            .NotEmpty()
            .Length(3)
            .Must(s=> IsAllLetters(s));

        RuleFor(x => x.FriendlyName)
            .NotNull()
            .NotEmpty()
            .Matches(@"^[A-Za-z0-9\-_ ğüşıöçİĞÜŞÖÇ]+$")
            .MaximumLength(50);
    }

    public static bool IsAllLetters(string s)
    {
        foreach (char c in s)
        {
            if (!Char.IsLetter(c))
                return false;
        }
        return true;
    }
}


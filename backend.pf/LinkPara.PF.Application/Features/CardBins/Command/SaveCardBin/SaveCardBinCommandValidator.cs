using FluentValidation;

namespace LinkPara.PF.Application.Features.CardBins.Command.SaveCardBin;

public class SaveCardBinCommandValidator : AbstractValidator<SaveCardBinCommand>
{
    public SaveCardBinCommandValidator()
    {
        RuleFor(x => x.BinNumber)
            .NotNull().NotEmpty().MinimumLength(6).MaximumLength(8)
            .Matches(@"[0-9]+$")
            .WithMessage("Invalid bin number!");

        RuleFor(x => x.BankCode)
          .NotNull().NotEmpty().WithMessage("Bank code cant be empty!");

        RuleFor(x => x.CardType).IsInEnum();

        RuleFor(x => x.CardSubType).IsInEnum();

        RuleFor(x => x.CardBrand).IsInEnum();

        RuleFor(x => x.CardNetwork).IsInEnum();

        RuleFor(x => x.Country).NotEmpty().NotNull();

        RuleFor(x => x.CountryName).NotEmpty().NotNull();
    }
}

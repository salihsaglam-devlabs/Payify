using FluentValidation;

namespace LinkPara.PF.Application.Features.CardBins.Command.UpdateCardBin;

public class UpdateCardBinCommandValidator : AbstractValidator<UpdateCardBinCommand>
{
    public UpdateCardBinCommandValidator()
    {
        RuleFor(x => x.BinNumber)
            .NotNull().NotEmpty().MinimumLength(6).MaximumLength(8)
            .Matches(@"[0-9]+$")
            .WithMessage("Invalid bin number!");

        RuleFor(x => x.BankCode)
          .NotNull().NotEmpty();

        RuleFor(x => x.CardType).IsInEnum();

        RuleFor(x => x.CardSubType).IsInEnum();

        RuleFor(x => x.CardBrand).IsInEnum();

        RuleFor(x => x.CardNetwork).IsInEnum();

        RuleFor(x => x.Country).NotEmpty().NotNull();

        RuleFor(x => x.CountryName).NotEmpty().NotNull();
    }
}

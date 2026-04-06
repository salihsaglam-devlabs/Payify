using FluentValidation;
using Microsoft.Extensions.Localization;

namespace LinkPara.Emoney.Application.Features.Wallets.Queries.WithdrawPreview;

public class WithdrawPreviewQueryValidator : AbstractValidator<WithdrawPreviewQuery>
{
    private readonly IStringLocalizer _localizer;
    public WithdrawPreviewQueryValidator(IStringLocalizerFactory factory)
    {
        _localizer = factory.Create("Exceptions", "LinkPara.Emoney.API");

        RuleFor(x => x.ReceiverIBAN)
            .NotNull()
            .NotEmpty()
            .MaximumLength(26)
            .Matches(@"^TR\d{8}[A-Z0-9]{16}$");

        RuleFor(x => x.WalletNumber)
           .NotNull()
           .NotEmpty()
           .MaximumLength(10);

        RuleFor(x => x.UserId)
           .NotNull()
           .NotEmpty();

        RuleFor(x => x.Amount)
           .NotNull()
           .WithMessage(_localizer.GetString("AmountCanNotBeEmptyException").Value);

        RuleFor(x => x.Amount)
          .GreaterThan(0)
          .WithMessage(_localizer.GetString("InvalidAmountException").Value);

        RuleFor(x => x.Description)
           .MaximumLength(150);

        RuleFor(x => x.ReceiverName)
            .NotNull()
            .NotEmpty()
            .Matches(@"^[A-Za-z\ ğüşıöçİĞÜŞÖÇ]+$").WithMessage(_localizer.GetString("InvalidReceiverNameException").Value)
            .MaximumLength(150);
    }
}

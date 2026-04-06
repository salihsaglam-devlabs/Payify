using FluentValidation;
using Microsoft.Extensions.Localization;

namespace LinkPara.Emoney.Application.Features.Wallets.Commands.Transfer;

public class TransferCommandValidator : AbstractValidator<TransferCommand>
{
    private readonly IStringLocalizer _localizer;
    public TransferCommandValidator(IStringLocalizerFactory factory)
    {
        _localizer = factory.Create("Exceptions", "LinkPara.Emoney.API");

        RuleFor(x => x.Amount)
            .NotNull()
            .WithMessage(_localizer.GetString("AmountCanNotBeEmptyException").Value);

        RuleFor(x => x.Amount)
          .GreaterThan(0)
          .WithMessage(_localizer.GetString("InvalidAmountException").Value);

        RuleFor(x => x.SenderWalletNumber)
            .MinimumLength(6)
            .MaximumLength(10)
            .NotEmpty();
        
        RuleFor(x => x.ReceiverWalletNumber)
            .MinimumLength(6)
            .MaximumLength(50)
            .NotEmpty();

        RuleFor(x => x.Description)
           .MaximumLength(150);

        RuleFor(x => x.UserId)
            .NotNull()
            .NotEmpty();
    }
}
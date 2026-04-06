using FluentValidation;
using Microsoft.Extensions.Localization;

namespace LinkPara.Emoney.Application.Features.Wallets.Queries.TransferPreview;

public class TransferPreviewQueryValidator : AbstractValidator<TransferPreviewQuery>
{
    private readonly IStringLocalizer _localizer;
    public TransferPreviewQueryValidator(IStringLocalizerFactory factory)
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
            .NotEmpty();

        RuleFor(x => x.ReceiverWalletNumber)
            .MinimumLength(6)
            .NotEmpty();

        RuleFor(x => x.Description)
           .MaximumLength(150);

        RuleFor(x => x.UserId)
            .NotNull()
            .NotEmpty();
    }
}

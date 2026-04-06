using FluentValidation;

namespace LinkPara.CampaignManagement.Application.Features.IWalletQrCodes.Commands.CreateQrCode;

public class CreateQrCodeCommandValidator : AbstractValidator<CreateQrCodeCommand>
{
    public CreateQrCodeCommandValidator()
    {
        RuleFor(x => x.WalletNumber)
            .NotNull()
            .NotEmpty();
        RuleFor(x => x.UserId)
            .NotNull()
            .NotEmpty();
    }
}

using FluentValidation;

namespace LinkPara.CampaignManagement.Application.Features.IWalletOtpCodes.Commands;

public class SendIWalletOtpCodeCommandValidator : AbstractValidator<SendIWalletOtpCodeCommand>
{
    public SendIWalletOtpCodeCommandValidator()
    {
        RuleFor(x => x.WalletNumber)
            .NotEmpty()
            .NotEmpty();
        RuleFor(x => x.Amount)
            .NotEmpty()
            .NotEmpty();
        RuleFor(x => x.MerchantName)
            .NotEmpty()
            .NotEmpty();
        RuleFor(x => x.OtpPassword)
            .NotEmpty()
            .NotEmpty();
        RuleFor(x => x.Type)
            .NotEmpty()
            .NotEmpty()
            .GreaterThan(0);
    }
}

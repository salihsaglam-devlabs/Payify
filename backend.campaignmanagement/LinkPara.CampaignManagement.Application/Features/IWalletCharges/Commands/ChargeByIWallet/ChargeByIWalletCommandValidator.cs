
using FluentValidation;

namespace LinkPara.CampaignManagement.Application.Features.IWalletCharges.Commands.ChargeByIWallet;

public class ChargeByIWalletCommandValidator : AbstractValidator<ChargeByIWalletCommand>
{
    public ChargeByIWalletCommandValidator()
    {
        RuleFor(s => s.WalletId)
            .NotEmpty()
            .NotNull(); 
        RuleFor(s => s.TerminalId)
            .NotEmpty()
            .NotNull();
        RuleFor(s => s.TerminalName)
            .NotEmpty()
            .NotNull();
        RuleFor(s => s.QrCode)
            .NotEmpty()
            .NotNull()
            .GreaterThan(0);
        RuleFor(s => s.Amount)
            .NotEmpty()
            .NotNull()
            .GreaterThan(0);
    }
}
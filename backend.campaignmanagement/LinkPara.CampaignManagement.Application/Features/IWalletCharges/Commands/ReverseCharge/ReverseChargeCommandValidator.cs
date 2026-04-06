
using FluentValidation;

namespace LinkPara.CampaignManagement.Application.Features.IWalletCharges.Commands.ReverseCharge;

public class ReverseChargeCommandValidator : AbstractValidator<ReverseChargeCommand>
{
    public ReverseChargeCommandValidator()
    {
        RuleFor(s => s.ProcessGuid).NotEmpty().NotNull();
        RuleFor(s => s.ReversedAmount).GreaterThan(0);
        RuleFor(s => s.CashBackAmount).GreaterThan(0);
    }
}

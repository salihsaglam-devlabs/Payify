

using FluentValidation;

namespace LinkPara.CampaignManagement.Application.Features.IWalletCashbacks.Commands.CashBack;

public class CashbackCommandValidator : AbstractValidator<CashBackCommand>
{
    public CashbackCommandValidator()
    {
        RuleFor(s => s.merchant_name).NotEmpty().NotNull();
        RuleFor(s => s.sales_transactions).NotEmpty().NotNull();
        RuleFor(s => s.reward_transactions).NotEmpty().NotNull();
    }
}

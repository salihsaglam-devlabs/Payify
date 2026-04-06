
using LinkPara.CampaignManagement.Application.Features.IWalletCashbacks.Commands.CashBack;

namespace LinkPara.CampaignManagement.Application.Commons.Interfaces;

public interface IIWalletCashbackService
{
    Task SaveCashBackTransactionAsync(CashBackCommand request);
}

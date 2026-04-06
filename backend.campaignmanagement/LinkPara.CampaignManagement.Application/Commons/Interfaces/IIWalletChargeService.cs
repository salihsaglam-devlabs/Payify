using LinkPara.CampaignManagement.Application.Features.IWalletCharges;
using LinkPara.CampaignManagement.Application.Features.IWalletCharges.Commands.ChargeByIWallet;
using LinkPara.CampaignManagement.Application.Features.IWalletCharges.Commands.ReverseCharge;
using LinkPara.CampaignManagement.Application.Features.IWalletCharges.Queries.GetCharges;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.CampaignManagement.Application.Commons.Interfaces;

public interface IIWalletChargeService
{
    Task<PaginatedList<ChargeTransactionDto>> GetChargeTransactionsAsync(GetChargeTransactionsSearchQuery request);
    Task ReverseChargeAsync(ReverseChargeCommand request);
    Task<Guid> SaveChargeAsync(ChargeByIWalletCommand request);
}

using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Entities.PhysicalPos;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface IIksPfService
{
    Task IKSSaveMerchantAsync(Merchant merchant);
    Task IKSSaveTerminalAsync(Merchant merchant);
    Task IKSUpdateTerminalAsync(Merchant merchant, MerchantVpos merchantVpos);
    Task UpdateMerchantStatus(Merchant merchant);
    Task IKSCreateTerminalAsync(Merchant merchant);
    Task IKSCreatePhysicalTerminalAsync(Merchant merchant);
    Task<bool> IKSUpdatePhysicalTerminalAsync(Merchant merchant, MerchantPhysicalPos merchantPhysicalPos);
    Task<string> BrandSharing(int bankCode);
}

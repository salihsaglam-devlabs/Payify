using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class MerchantWallet : AuditEntity, ITrackChange
{
    public string WalletNumber { get; set; }
    public Guid MerchantId { get; set; }
    public Merchant Merchant { get; set; }
}
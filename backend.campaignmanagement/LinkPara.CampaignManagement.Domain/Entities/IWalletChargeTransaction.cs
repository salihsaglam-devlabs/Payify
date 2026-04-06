
using LinkPara.CampaignManagement.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.CampaignManagement.Domain.Entities;

public class IWalletChargeTransaction : AuditEntity
{
    public Guid IWalletChargeId { get; set; }
    public ChargeTransactionType ChargeTransactionType { get; set; }
    public decimal Amount { get; set; }
    public IWalletCharge IWalletCharge { get; set; }
    public string ProvisionReferenceNumber { get; set; }
    public string ProvisionConversationId { get; set; }
    public int OrderId { get; set; }
}

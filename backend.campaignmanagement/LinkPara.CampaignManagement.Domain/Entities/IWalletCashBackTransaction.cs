using LinkPara.CampaignManagement.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.CampaignManagement.Domain.Entities;

public class IWalletCashbackTransaction : AuditEntity
{
    public Guid IWalletChargeId { get; set; }
    public IWalletTransactionType TransactionType { get; set; }
    public int Oid { get; set; }
    public decimal Amount { get; set; }
    public decimal Balance { get; set; }
    public string ExternalStatus { get; set; }
    public decimal? VatRate { get; set; }
    public decimal CommissionRate { get; set; }
    public int ExternalOrderId { get; set; }
    public int IWalletCardId { get; set; }
    public int WalletId { get; set; }
    public int? MerchantId { get; set; }
    public string MerchantName { get; set; }
    public int? MerchantBranchId { get; set; }
    public string MerchantBranchName { get; set; }
    public int PosId { get; set; }
    public int CustomerId { get; set; }
    public int CustomerBranchId { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal CommissionAmount { get; set; }
    public string LoadType { get; set; }
    public int? QrCode { get; set; }
    public string WalletNumber { get; set; }
    public string HashData { get; set; }
    public IWalletCharge IWalletCharge { get; set; }
}

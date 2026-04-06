using LinkPara.CampaignManagement.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.CampaignManagement.Domain.Entities;

public class IWalletCharge : AuditEntity
{
    public string WalletId { get; set; }
    public int TerminalId { get; set; }
    public string TerminalName { get; set; }
    public int CurrencyCode { get; set; }
    public int QrCode { get; set; }
    public decimal Amount { get; set; }
    public ChargeStatus ChargeStatus { get; set; }
    public string ExceptionMessage { get; set; }
    public string WalletNumber { get; set; }
    public Guid UserId { get; set; }
    public int? MerchantId { get; set; }
    public string MerchantName { get; set; }
    public int? MerchantBranchId { get; set; }
    public string MerchantBranchName { get; set; }
    public SourceCampaignType SourceCampaignType { get; set; }
    public Guid IWalletQrCodeId { get; set; }
    public IWalletQrCode IWalletQrCode { get; set; }

}

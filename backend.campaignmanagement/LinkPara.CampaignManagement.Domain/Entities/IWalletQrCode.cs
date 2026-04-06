using LinkPara.CampaignManagement.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.CampaignManagement.Domain.Entities;

public class IWalletQrCode : AuditEntity
{
    public Guid UserId { get; set; }
    public string WalletNumber { get; set; }
    public int CardId { get; set; }
    public string CardNumber { get; set; }
    public int QrCode { get; set; }
    public DateTime ExpiresIn {  get; set; }
    public string Message { get; set; }
    public IWalletQrCodeStatus IWalletQrCodeStatus { get; set; }
    public Guid IWalletCardId { get; set; }
    public IWalletCard IWalletCard { get; set; }
}

using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class CardToken : AuditEntity
{
    public string Token { get; set; }
    public DateTime ExpiryDate { get; set; }
    public Guid MerchantId { get; set; }
    public Merchant Merchant { get; set; }
    public string CvvEncrypted { get; set; }
    public string CardNumberEncrypted { get; set; }
    public string ExpireDateEncrypted { get; set; }
}
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.DomainEvents;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class MerchantReturnPool : AuditEntity, ITrackChange
{
    public DateTime ActionDate { get; set; }
    public Guid ActionUser { get; set; }
    public ReturnStatus ReturnStatus { get; set; }
    public decimal Amount { get; set; }
    public string OrderId { get; set; }
    public string ConversationId { get; set; }
    public string ClientIpAddress { get; set; }
    public string LanguageCode { get; set; }
    public Guid MerchantId { get; set; }
    public Merchant Merchant { get; set; }
    public string CardNumber { get; set; }
    public int BankCode { get; set; }
    public string BankName { get; set; }
    public string RejectDescription { get; set; }
    public string RejectReason { get; set; }
    public bool? BankStatus { get; set; }
    public string CurrencyCode { get; set; }
    public string BankResponseCode { get; set; }
    public string BankResponseDescription { get; set; }
    public bool? IsTopUpPayment { get; set; }
}
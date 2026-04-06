using LinkPara.Emoney.Application.Commons.Enums;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Emoney.Domain.Entities;

public class CardTopupRequest : AuditEntity
{
    public decimal Amount { get; set; }
    public decimal CommissionRate { get; set; }
    public decimal CommissionTotal { get; set; }
    public decimal BsmvTotal { get; set; }
    public decimal Fee { get; set; }
    public string Currency { get; set; }
    public CardBrand CardBrand { get; set; }  
    public string CardNumber { get; set; }
    public CardType CardType { get; set; }
    public string ThreedSessionId { get; set; }
    public string ProvisionNumber { get; set; }
    public string OrderId { get; set; }
    public string ConversationId { get; set; }
    public string ErrorMessage { get; set; }
    public string ErrorCode { get; set; }
    public Guid WalletId { get; set; }
    public string WalletNumber { get; set; }
    public string Name { get; set; }
    public string CancelDescription { get; set; }
    public int? MdStatus { get; set; }
    public string AccountKey { get; set; }
    public DateTime? TransactionDate { get; set; }
    public CardTopupRequestStatus Status { get; set; }
    public PaymentProviderType PaymentProviderType { get; set; }
    public AuthenticationMethod AuthenticationMethod { get; set; }
    public Secure3dType Secure3dType { get; set; }
    public string Description { get; set; }
    public string BankName { get; set; }
    public int BankCode { get; set; }
}
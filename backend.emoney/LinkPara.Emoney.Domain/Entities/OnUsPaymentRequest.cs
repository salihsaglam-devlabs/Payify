using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Emoney.Domain.Entities;

public class OnUsPaymentRequest : AuditEntity
{
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public string MerchantName { get; set; }
    public string MerchantNumber { get; set; }    
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public OnUsPaymentStatus Status { get; set; }
    public string ChargebackDescription { get; set; }
    public string PhoneCode { get; set; }
    public string PhoneNumber { get; set; }
    public string WalletNumber { get; set; }
    public Guid WalletId { get; set; }
    public Guid TransactionId{ get; set; }
    public DateTime TransactionDate { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public string CancelDescription { get; set; }
    public string ConversationId { get; set; }
    public string OrderId { get; set; }
    public DateTime ExpireDate { get; set; }
    public DateTime RequestDate { get; set; }
    public string MerchantTransactionId { get; set; }    
}

 
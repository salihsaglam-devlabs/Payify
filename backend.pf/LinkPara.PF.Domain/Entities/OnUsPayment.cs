using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class OnUsPayment : AuditEntity
{
    public ChannelStatus Status { get; set; }
    public ChannelPaymentStatus PaymentStatus { get; set; }
    public WebhookStatus WebhookStatus { get; set; }
    public Guid MerchantId { get; set; }
    public Guid MerchantTransactionId { get; set; }
    public string MerchantName { get; set; }
    public string MerchantNumber { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Email { get; set; }
    public string PhoneCode { get; set; }
    public string PhoneNumber { get; set; }
    public string WalletNumber { get; set; }
    public string EmoneyReferenceNumber { get; set; }
    public Guid EmoneyTransactionId { get; set; }
    public DateTime ExpiryDate { get; set; }
    public int WebhookRetryCount { get; set; }
    public string CallbackUrl { get; set; }
    public string OrderId { get; set; }
    public decimal Amount { get; set; }
    public int Currency { get; set; }
}
using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Emoney.Domain.Entities;

public class WalletPaymentRequest : AuditEntity
{
    public string PaymentReferenceId {  get; set; }
    public Guid InternalTransactionId { get; set; }
    public WalletPaymentStatus Status { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; }
    public string SenderWalletNo { get; set; }
    public string SenderName { get; set; }
    public string ReceiverWalletNo { get; set; }
    public string ReceiverName { get; set; }
    public DateTime TransactionDate { get; set; }
    public bool IsLoggedIn { get; set; }
}
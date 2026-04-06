using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Emoney.Domain.Entities;

public class Transaction : AuditEntity
{
    public TransactionDirection TransactionDirection { get; set; }
    public TransactionType TransactionType { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public TransactionStatus TransactionStatus { get; set; }
    public string CurrencyCode { get; set; }
    public Currency Currency { get; set; }
    public decimal Amount { get; set; }
    public decimal PreBalance { get; set; }
    public decimal CurrentBalance { get; set; }
    public string Tag { get; set; }
    public string TagTitle { get; set; }
    public DateTime TransactionDate { get; set; }
    public DateTime? ExternalTransactionDate { get; set; }
    public string ExternalReferenceId { get; set; }
    public Guid? RelatedTransactionId { get; set; }
    public string Description { get; set; }
    public string PaymentType { get; set; }
    public Guid WalletId { get; set; }
    public Wallet Wallet { get; set; }
    public Guid? IncomingTransactionId { get; set; }
    public Guid? CardTopupRequestId { get; set; }
    public Guid? WithdrawRequestId { get; set; }
    public int? ReceiverBankCode { get; set; }
    public string ReceiverName { get; set; }
    public int? SenderBankCode { get; set; }
    public string SenderName { get; set; }
    public string SenderAccountNumber { get; set; }
    public Guid? ReturnedTransactionId { get; set; }
    public Guid? CounterWalletId { get; set; }
    public string Channel { get; set; }
    public virtual BulkTransferDetail BulkTransferDetail { get; set; }
    public string ReceiptNumber { get; set; }
    public bool IsReturned { get; set; }
    public string IpAddress { get; set; }
    public bool IsCancelled { get; set; }
    public string CustomerTransactionId { get; set; }
    public string PaymentChannel { get; set; }
    public bool IsSettlementReceived { get; set; }
    public decimal UsedCreditAmount { get; set; }
    public string IdempotentKey { get; set; }
}
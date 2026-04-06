using LinkPara.SharedModels.Persistence;

namespace LinkPara.Emoney.Application.Features.Transactions;

public class CustomerTransactionDto
{
    public Guid Id { get; set; }
    public string TransactionType { get; set; }
    public string PaymentMethod { get; set; }
    public string TransactionStatus { get; set; }
    public string TransactionDirection { get; set; }
    public string CurrencyCode { get; set; }
    public decimal Amount { get; set; }
    public decimal PreBalance { get; set; }
    public decimal CurrentBalance { get; set; }
    public string Tag { get; set; }
    public string TagTitle { get; set; }
    public DateTime TransactionDate { get; set; }
    public string Description { get; set; }
    public string PaymentType { get; set; }
    public string ExternalReferenceId { get; set; }
    public Guid? RelatedTransactionId { get; set; }
    public Guid WalletId { get; set; }
    public Guid? IncomingTransactionId { get; set; }
    public Guid? WithdrawRequestId { get; set; }
    public Guid? CardTopupRequestId { get; set; }
    public int? ReceiverBankCode { get; set; }
    public string ReceiverName { get; set; }
    public int? SenderBankCode { get; set; }
    public string SenderName { get; set; }
    public string SenderAccountNumber { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime? UpdateDate { get; set; }
    public string CreatedBy { get; set; }
    public string LastModifiedBy { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public Guid? ReturnedTransactionId { get; set; }
    public string ReceiptNumber { get; set; }
    public bool IsReturned { get; set; }
    public string IpAddress { get; set; }
    public string Channel { get; set; }
    public bool IsCancelled { get; set; }
    public string CustomerTransactionId { get; set; }
    public string PaymentChannel { get; set; }
    public bool IsSettlementReceived { get; set; }
    public decimal UsedCreditAmount { get; set; }
}

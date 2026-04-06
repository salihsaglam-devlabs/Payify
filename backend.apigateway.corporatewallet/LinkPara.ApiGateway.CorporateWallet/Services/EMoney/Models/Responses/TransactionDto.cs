using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Enums;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Responses;

public class TransactionDto
{
    public Guid Id { get; set; }
    public TransactionType TransactionType { get; set; }
    public TransactionDirection TransactionDirection { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public TransactionStatus TransactionStatus { get; set; }
    public string CurrencySymbol { get; set; }
    public string CurrencyCode { get; set; }
    public decimal Amount { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal PreBalance { get; set; }
    public decimal CurrentBalance { get; set; }
    public string Tag { get; set; }
    public string TagTitle { get; set; }
    public DateTime TransactionDate { get; set; }
    public string Description { get; set; }
    public string ExternalReferenceId { get; set; }
    public Guid? RelatedTransactionId { get; set; }
    public Guid WalletId { get; set; }
    public Guid? IncomingTransactionId { get; set; }
    public Guid? WithdrawRequestId { get; set; }
    public int? ReceiverBankCode { get; set; }
    public string ReceiverName { get; set; }
    public string ReceiverIban { get; set; }
    public int? SenderBankCode { get; set; }
    public string SenderName { get; set; }
    public string SenderAccountNumber { get; set; }
    public string WalletNumber { get; set; }
    public string WalletName { get; set; }
    public string CounterWalletNumber { get; set; }
    public string CounterWalletName { get; set; }
    public decimal TotalAmount { get; set; }
    public string ReceiptNumber { get; set; }
}


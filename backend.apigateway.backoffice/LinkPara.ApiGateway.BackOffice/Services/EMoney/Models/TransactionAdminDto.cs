using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Enums;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models;

public class TransactionAdminDto
{
    public Guid Id { get; set; }
    public TransactionType TransactionType { get; set; }
    public TransactionDirection TransactionDirection { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public TransactionStatus TransactionStatus { get; set; }
    public CardType CardType { get; set; }
    public string CurrencySymbol { get; set; }
    public CurrencyDto Currency { get; set; }
    public decimal Amount { get; set; }
    public decimal? CommissionAmount { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal PreBalance { get; set; }
    public decimal CurrentBalance { get; set; }
    public string Tag { get; set; }
    public string TagTitle { get; set; }
    public DateTime TransactionDate { get; set; }
    public DateTime? ExternalTransactionDate { get; set; }
    public string ExternalReferenceId { get; set; }
    public Guid? RelatedTransactionId { get; set; }
    public string Description { get; set; }
    public Guid WalletId { get; set; }
    public WalletDto Wallet { get; set; }
    public Guid? IncomingTransactionId { get; set; }
    public Guid? WithdrawRequestId { get; set; }
    public int? ReceiverBankCode { get; set; }
    public string ReceiverName { get; set; }
    public string ReceiverIban { get; set; }
    public int? SenderBankCode { get; set; }
    public string SenderName { get; set; }
    public string SenderAccountNumber { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime? UpdateDate { get; set; }
    public string CreatedBy { get; set; }
    public string LastModifiedBy { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public string CounterWalletNumber { get; set; }
    public string CounterWalletName { get; set; }
    public decimal TotalAmount { get; set; }
    public Guid? ReturnedTransactionId { get; set; }
    public string ReceiptNumber { get; set; }
    public string TotalAmountText { get; set; }
    public bool IsReturned { get; set; }
    public string Channel { get; set; }
    public string PaymentType { get; set; }
}

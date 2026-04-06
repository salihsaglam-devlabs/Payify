using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Enums;
using LinkPara.SharedModels.Banking.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;

public class IncomingTransactionDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; }
    public DateTime TransactionDate { get; set; }
    public string BankReferenceNumber { get; set; }
    public string QueryNumber { get; set; }
    public string SenderIbanNumber { get; set; }
    public string SenderName { get; set; }
    public string SenderTaxNumber { get; set; }
    public int SenderBankCode { get; set; }
    public string SenderBranchCode { get; set; }
    public int ReceiverBankCode { get; set; }
    public string ErrorMessage { get; set; }
    public string Description { get; set; }
    public Guid MoneyTransferTransactionId { get; set; }
    public BankDebitCreditType BankDebitCreditType { get; set; }
    public string BankTransactionChannel { get; set; }
    public IncomingTransferType IncomingTransferType { get; set; }
    public string SenderBankName { get; set; }
    public string ReceiverBankName { get; set; }
    public Guid IncomingTransactionSummaryId { get; set; }
    public IncomingTransactionStatus Status { get; set; }
    public string CreatedBy { get; set; }
    public string LastModifiedBy { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime? UpdateDate { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public string ReceiverWalletNumber { get; set; }
    public string ReceiverName { get; set; }
    public string VirtualIbanNumber { get; set; }
}

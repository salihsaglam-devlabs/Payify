using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Enums;
using LinkPara.SharedModels.Banking.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models;

public class WithdrawRequestDto
{
    public Guid Id { get; set; }
    public DateTime CreateDate { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public WithdrawStatus WithdrawStatus { get; set; }
    public TransferType TransferType { get; set; }
    public string WalletNumber { get; set; }
    public Guid InternalTransactionId { get; set; }
    public string CurrencyCode { get; set; }
    public bool IsReceiverIbanOwned { get; set; }
    public string ReceiverIbanNumber { get; set; }
    public string ReceiverName { get; set; }
    public string ReceiverTaxNumber { get; set; }
    public int ReceiverBankCode { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; }
    public int TransactionBankCode { get; set; }
    public decimal BankReferenceNumber { get; set; }
    public decimal TransactionId { get; set; }
    public int QueryNumber { get; set; }
    public string TransactionResponse { get; set; }
    public decimal Bsmv { get; set; }
    public decimal Pricing { get; set; }
    public string ReceiverBankName { get; set; }
    public string TransactionBankName { get; set; }
    public DateTime? TransactionDate { get; set; }
    public string SenderName { get; set; }
    public Guid? MoneyTransferReferenceId { get; set; }
}

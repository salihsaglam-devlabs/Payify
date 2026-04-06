using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Banking.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Emoney.Domain.Entities;

public class WithdrawRequest : AuditEntity
{
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
    public DateTime MoneyTransferPaymentDate { get; set; }
    public MoneyTransferStatus MoneyTransferStatus { get; set; }
    public Guid MoneyTransferReferenceId { get; set; }
    public string ReceiverBankName { get; set; }
    public string TransactionBankName { get; set; }
    public bool IsProcessed { get; set; }
}

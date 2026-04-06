using LinkPara.SharedModels.Banking.Enums;

namespace LinkPara.HttpProviders.MoneyTransfer.Models;

public class SaveTransferRequest
{
    public TransactionSource Source { get; set; }
    public Guid TransactionSourceReferenceId { get; set; }
    public decimal Amount { get; set; }
    public string ReceiverIBAN { get; set; }
    public string ReceiverName { get; set; }
    public string Description { get; set; }
    public string CurrencyCode { get; set; }
    public Guid UserId { get; set; }
    public Guid? IncomingTransactionId { get; set; }
    public bool IsReturnPayment { get; set; }
    public DateTime? WorkingDate { get; set; }
    public string WalletNumber { get; set; }
}

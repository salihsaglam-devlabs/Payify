using LinkPara.SharedModels.Banking.Enums;

namespace LinkPara.SharedModels.BusModels.IntegrationEvents.MoneyTransfer;

public class TransferFailed
{
    public TransactionSource Source { get; set; }
    public TransferType TransferType { get; set; }
    public DateTime TransferDate { get; set; }
    public Guid TransactionSourceReferenceId { get; set; }
    public Guid MoneyTransferPaymentId { get; set; }
    public MoneyTransferStatus MoneyTransferStatus { get; set; }
    public string BankTransactionResponse { get; set; }
}

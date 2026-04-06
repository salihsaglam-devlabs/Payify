using LinkPara.SharedModels.Banking.Enums;

namespace LinkPara.SharedModels.BusModels.IntegrationEvents.MoneyTransfer;

public class TransferCompleted
{
    public TransactionSource Source { get; set; }
    public TransferType TransferType { get; set; }
    public DateTime TransferDate { get; set; }
    public Guid TransactionSourceReferenceId { get; set; }
    public Guid MoneyTransferPaymentId { get; set; }
    public MoneyTransferStatus MoneyTransferStatus { get; set; }
    public string BankReferenceNumber { get; set; }
    public string BankTransactionId { get; set; }
    public string BankQueryNumber { get; set; }
    public string BankTransactionResponse { get; set; }
    public bool IncomingTransferCompleted { get; set; }
}

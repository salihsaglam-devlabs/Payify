namespace LinkPara.SharedModels.BusModels.Commands.Emoney;

public class ReturnedWithdrawTransaction
{
    public decimal Amount { get; set; }
    public DateTime? CancelDate { get; set; }
    public string CancelDescription { get; set; }
    public DateTime TransactionDate { get; set; }
    public string BankTransactionId { get; set; }
    public string TransferReferenceId { get; set; }
    public Guid MoneyTransferReferenceId { get; set; }
}
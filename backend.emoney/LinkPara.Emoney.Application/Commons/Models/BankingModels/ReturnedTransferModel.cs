namespace LinkPara.Emoney.Application.Commons.Models.BankingModels;

public class ReturnedTransferModel
{
    public decimal Amount { get; set; }
    public DateTime? CancelDate { get; set; }
    public string CancelDescription { get; set; }
    public DateTime TransactionDate { get; set; }
    public string BankTransactionId { get; set; }
}
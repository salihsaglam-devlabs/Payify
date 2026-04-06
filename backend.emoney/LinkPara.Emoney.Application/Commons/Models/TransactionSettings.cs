namespace LinkPara.Emoney.Application.Commons.Models;

public class TransactionSettings
{
    public bool ReceiptNumberAssignment { get; set; }
    public int ReceiptTransactionLimit { get; set; }
    public string ReceiptNumberPrefix { get; set; }
}

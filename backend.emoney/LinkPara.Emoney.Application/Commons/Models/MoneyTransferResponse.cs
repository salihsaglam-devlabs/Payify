namespace LinkPara.Emoney.Application.Commons.Models;

public class MoneyTransferResponse
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public Guid TransactionId { get; set; }
}

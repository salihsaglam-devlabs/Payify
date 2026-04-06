namespace LinkPara.SharedModels.BusModels.IntegrationEvents.Emoney;

public class ReturnedTransactionProcessCompleted
{
    public bool IsSucceeded { get; set; }
    public string ErrorMessage { get; set; }
    public Guid MoneyTransferTransactionId { get; set; }
}

namespace LinkPara.SharedModels.BusModels.IntegrationEvents.Logging;

public class BankAccountActivity
{
    public Guid SummaryId { get; set; }
    public DateTime QueryDate { get; set; }
    public int BankCode { get; set; }
    public string TransactionDetail { get; set; }
}

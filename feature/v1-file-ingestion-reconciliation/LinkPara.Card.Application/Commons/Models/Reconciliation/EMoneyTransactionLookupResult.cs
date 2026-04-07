namespace LinkPara.Card.Application.Commons.Models.Reconciliation;

public class EMoneyTransactionLookupResult
{
    public EMoneyTransactionLookupStatus Status { get; set; }
    public string ExternalTransactionId { get; set; }
    public string TransactionState { get; set; }
    public decimal? Amount { get; set; }
    public string CurrencyCode { get; set; }
    public bool IsCancelled { get; set; }
    public bool IsSettlementReceived { get; set; }
    public DateTime? TransactionDate { get; set; }
}

public enum EMoneyTransactionLookupStatus
{
    Found = 1,
    NotFound = 2,
    Unavailable = 3
}

namespace LinkPara.Billing.Application.Commons.Models.Reconciliation;

public class Reconciliation
{
    public decimal PaymentAmount { get; set; }
    public decimal CancellationAmount { get; set; }
    public int PaymentCount { get; set; }
    public int CancellationCount { get; set; }
}
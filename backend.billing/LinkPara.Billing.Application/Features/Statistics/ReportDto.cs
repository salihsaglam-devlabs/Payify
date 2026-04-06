namespace LinkPara.Billing.Application.Features.Statistics;

public class ReportDto
{
    public int Succeeded { get; set; }
    public decimal SucceededAmount { get; set; }
    public int Failed { get; set; }
    public decimal FailedAmount { get; set; }
    public int PendingReconciliation { get; set; }
}

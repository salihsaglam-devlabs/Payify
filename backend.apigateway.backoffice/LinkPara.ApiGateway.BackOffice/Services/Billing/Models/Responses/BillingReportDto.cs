namespace LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Responses;

public class BillingReportDto
{
    public int Succeeded { get; set; }
    public decimal SucceededAmount { get; set; }
    public int Failed { get; set; }
    public decimal FailedAmount { get; set; }
    public int PendingReconciliation { get; set; }
}

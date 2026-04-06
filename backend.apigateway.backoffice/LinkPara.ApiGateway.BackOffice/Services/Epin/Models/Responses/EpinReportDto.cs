namespace LinkPara.ApiGateway.BackOffice.Services.Epin.Models.Responses;

public class EpinReportDto
{
    public int Succeeded { get; set; }
    public decimal SucceededAmount { get; set; }
    public int Failed { get; set; }
    public decimal FailedAmount { get; set; }
    public int PendingReconciliation { get; set; }
}

namespace LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Requests;

public class BillCancelRequest
{
    public Guid TransactionId { get; set; }
    public string CancellationReason { get; set; }
}
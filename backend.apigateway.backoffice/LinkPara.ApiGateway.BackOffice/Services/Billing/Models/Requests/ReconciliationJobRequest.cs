namespace LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Requests;

public class ReconciliationJobRequest
{
    public Guid VendorId { get; set; }
    public DateTime ReconciliationDate { get; set; }
}
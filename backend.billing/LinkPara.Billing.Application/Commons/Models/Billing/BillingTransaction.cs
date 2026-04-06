namespace LinkPara.Billing.Application.Commons.Models.Billing;

public class BillingTransaction
{
    public string RequestId { get; set; }
    public Guid InstitutionId { get; set; }
    public Guid VendorId { get; set; }
}
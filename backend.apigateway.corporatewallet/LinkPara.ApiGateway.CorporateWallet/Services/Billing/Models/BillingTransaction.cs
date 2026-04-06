namespace LinkPara.ApiGateway.CorporateWallet.Services.Billing.Models;

public class BillingTransaction
{
    public string RequestId { get; set; }
    public Guid InstitutionId { get; set; }
    public Guid VendorId { get; set; }
}
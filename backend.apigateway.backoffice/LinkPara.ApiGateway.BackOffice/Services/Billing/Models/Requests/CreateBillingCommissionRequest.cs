namespace LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Requests;

public class CreateBillingCommissionRequest
{
    public Guid VendorId { get; set; }
    public PaymentSource PaymentType { get; set; }
    public Guid InstitutionId { get; set; }
    public decimal Rate { get; set; }
    public decimal Fee { get; set; }
    public decimal MinValue { get; set; }
    public decimal MaxValue { get; set; }
}
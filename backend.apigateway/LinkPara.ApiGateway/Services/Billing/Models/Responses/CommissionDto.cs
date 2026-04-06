namespace LinkPara.ApiGateway.Services.Billing.Models.Responses;

public class CommissionDto
{
    public Guid VendorId { get; set; }
    public BillPaymentSource PaymentType { get; set; }
    public Guid InstitutionId { get; set; }
    public decimal Rate { get; set; }
    public decimal Fee { get; set; }
    public int MinValue { get; set; }
    public int MaxValue { get; set; }
}
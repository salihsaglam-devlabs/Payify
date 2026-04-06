namespace LinkPara.ApiGateway.Services.Billing.Models.Requests;

public class CommissionFilterRequest
{
    public Guid InstitutionId { get; set; }
    public decimal Amount { get; set; }
    public BillPaymentSource PaymentSource { get; set; }
}
namespace LinkPara.ApiGateway.Services.Billing.Models.Requests;

public class BillPaymentRequest
{
    public string RequestId { get; set; }
    public Guid InstitutionId { get; set; }
    public Bill Bill { get; set; }
    public string PayeeFullName { get; set; }
    public string PayeeMobile { get; set; }
    public string PayeeEmail { get; set; }
    public string WalletNumber { get; set; }
    public BillPaymentSource PaymentSource { get; set; }
}

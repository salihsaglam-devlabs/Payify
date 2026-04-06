namespace LinkPara.ApiGateway.CorporateWallet.Services.Billing.Models.Requests;
public class BillPreviewRequest
{
    public Guid InstitutionId { get; set; }
    public string WalletNumber { get; set; }
    public string PayeeFullName { get; set; }
    public string PayeeMobile { get; set; }
    public string PayeeEmail { get; set; }
    public BillPaymentSource PaymentSource { get; set; }
    public string RequestId { get; set; }
    public Bill Bill { get; set; }
}

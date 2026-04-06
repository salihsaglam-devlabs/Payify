namespace LinkPara.ApiGateway.CorporateWallet.Services.Billing.Models;

public class BillInquiryResponse : BillingTransaction
{
    public List<Bill> Bills { get; set; }
}
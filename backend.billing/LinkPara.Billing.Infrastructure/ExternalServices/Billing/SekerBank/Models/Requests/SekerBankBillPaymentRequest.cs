namespace LinkPara.Billing.Infrastructure.ExternalServices.Billing.SekerBank.Models.Requests;

public class SekerBankBillPaymentRequest : SekerBankBillingTransaction
{
    public List<SekerBankBill> billsToPay { get; set; }
}
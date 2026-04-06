namespace LinkPara.Billing.Infrastructure.ExternalServices.Billing.SekerBank.Models.Responses;

public class SekerBankBillPaymentResponse : SekerBankBillingTransaction
{
    public List<SekerBankBillInvoice> billsToPay { get; set; }
}
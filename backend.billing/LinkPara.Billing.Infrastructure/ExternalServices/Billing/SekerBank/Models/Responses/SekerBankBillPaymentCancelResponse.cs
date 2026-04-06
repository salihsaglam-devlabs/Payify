namespace LinkPara.Billing.Infrastructure.ExternalServices.Billing.SekerBank.Models.Responses;

public class SekerBankBillPaymentCancelResponse : SekerBankBillingTransaction
{
    public List<SekerBankBillCancelledInvoice> billsToCancel { get; set; }
}
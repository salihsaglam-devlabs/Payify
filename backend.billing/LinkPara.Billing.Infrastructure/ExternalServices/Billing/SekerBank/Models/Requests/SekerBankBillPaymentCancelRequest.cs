namespace LinkPara.Billing.Infrastructure.ExternalServices.Billing.SekerBank.Models.Requests;

public class SekerBankBillPaymentCancelRequest : SekerBankBillingTransaction
{
    public List<SekerBankBillToCancel> billsToCancel { get; set; }
}
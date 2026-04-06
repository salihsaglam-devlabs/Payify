namespace LinkPara.Billing.Infrastructure.ExternalServices.Billing.SekerBank.Models.Requests;

public class SekerBankBillStatusRequest : SekerBankBillingTransaction
{
    public string billOid { get; set; }
}
namespace LinkPara.Billing.Infrastructure.ExternalServices.Billing.SekerBank.Models.Requests;

public class SekerBankReconciliationDetailsRequest
{
    public string reconciliationDate { get; set; }
    public string institutionOid { get; set; }
}
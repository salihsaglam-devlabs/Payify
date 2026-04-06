namespace LinkPara.Billing.Infrastructure.ExternalServices.Billing.SekerBank.Models.Requests;

public class SekerBankInquireInstitutionReconciliationRequest
{
    public string reconciliationDate { get; set; }
    public List<SekerBankInstitutionReconciliation> details { get; set; }
}

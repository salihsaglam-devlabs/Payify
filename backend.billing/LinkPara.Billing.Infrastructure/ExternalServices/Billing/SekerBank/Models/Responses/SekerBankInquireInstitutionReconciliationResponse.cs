namespace LinkPara.Billing.Infrastructure.ExternalServices.Billing.SekerBank.Models.Responses;

public class SekerBankInquireInstitutionReconciliationResponse
{
    public string reconciliationDate { get; set; }
    public List<SekerBankInstitutionReconciliationStatus> details { get; set; }
}
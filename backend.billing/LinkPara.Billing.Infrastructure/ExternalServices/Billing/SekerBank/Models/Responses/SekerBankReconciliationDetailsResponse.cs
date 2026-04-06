namespace LinkPara.Billing.Infrastructure.ExternalServices.Billing.SekerBank.Models.Responses;

public class SekerBankReconciliationDetailsResponse
{
    public string reconciliationDate { get; set; }
    public List<SekerBankInstitutionReconciliation> details { get; set; }
}
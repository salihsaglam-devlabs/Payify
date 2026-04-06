namespace LinkPara.Billing.Infrastructure.ExternalServices.Billing.SekerBank.Models;

public class SekerBankInstitutionReconciliationStatus : SekerBankInstitutionReconciliation
{
    public string reconciliationDate { get; set; }
    public string reconciliationStatus { get; set; }
}
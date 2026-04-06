namespace LinkPara.Billing.Infrastructure.ExternalServices.Billing.SekerBank.Models.Responses;

public class SekerBankInstitutionReconciliationDetailsResponse
{
    public string reconciliationDate { get; set; }
    public SekerBankInstitutionReconciliation detail { get; set; }
}
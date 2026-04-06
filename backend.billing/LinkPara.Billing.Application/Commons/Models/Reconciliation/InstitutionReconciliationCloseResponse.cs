namespace LinkPara.Billing.Application.Commons.Models.Reconciliation;

public class InstitutionReconciliationCloseResponse
{
    public DateTime ReconciliationDate { get; set; }
    public List<InstitutionReconciliationCloseDetails> InstitutionReconciliations { get; set; }
}
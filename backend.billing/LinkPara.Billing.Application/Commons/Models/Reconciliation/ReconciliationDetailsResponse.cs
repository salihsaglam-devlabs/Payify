namespace LinkPara.Billing.Application.Commons.Models.Reconciliation;

public class ReconciliationDetailsResponse
{
    public DateTime ReconciliationDate { get; set; }
    public List<InstitutionReconciliation> ReconciliationDetails { get; set; }
}
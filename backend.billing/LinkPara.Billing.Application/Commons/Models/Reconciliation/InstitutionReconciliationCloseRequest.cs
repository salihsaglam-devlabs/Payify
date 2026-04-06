namespace LinkPara.Billing.Application.Commons.Models.Reconciliation;

public class InstitutionReconciliationCloseRequest
{
    public Guid VendorId { get; set; }
    public DateTime ReconciliationDate { get; set; }
    public List<InstitutionReconciliation> InstitutionReconciliations { get; set; }
}
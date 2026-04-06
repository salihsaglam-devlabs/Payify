using LinkPara.Billing.Domain.Enums;

namespace LinkPara.Billing.Application.Commons.Models.Reconciliation;

public class InstitutionReconciliation : Reconciliation
{
    public Guid InstitutionSummaryId { get; set; }
    public Guid InstitutionId { get; set; }
    public ReconciliationStatus ReconciliationStatus { get; set; }
}
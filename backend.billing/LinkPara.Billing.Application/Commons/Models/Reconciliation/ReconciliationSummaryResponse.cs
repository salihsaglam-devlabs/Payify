using LinkPara.Billing.Domain.Enums;

namespace LinkPara.Billing.Application.Commons.Models.Reconciliation;

public class ReconciliationSummaryResponse : Reconciliation
{
    public DateTime ReconciliationDate { get; set; }
    public ReconciliationStatus ReconciliationStatus { get; set; }
}
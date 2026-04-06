using LinkPara.Billing.Application.Commons.Mappings;
using LinkPara.Billing.Application.Features.Reconciliations.Commands.ReconcilationSummary;

namespace LinkPara.Billing.Application.Commons.Models.Reconciliation;

public class ReconciliationSummaryRequest : IMapFrom<PerformReconcilationSummaryCommand>
{
    public DateTime ReconciliationDate { get; set; }
    public Guid VendorId { get; set; }
}
using LinkPara.Billing.Application.Commons.Mappings;
using LinkPara.Billing.Application.Features.Reconciliations.Commands.ReconcilationDetail;

namespace LinkPara.Billing.Application.Commons.Models.Reconciliation;

public class ReconciliationDetailsRequest : IMapFrom<PerformReconciliationDetailCommand>
{
    public DateTime ReconciliationDate { get; set; }
    public Guid InstitutionId { get; set; }
    public Guid VendorId { get; set; }
}
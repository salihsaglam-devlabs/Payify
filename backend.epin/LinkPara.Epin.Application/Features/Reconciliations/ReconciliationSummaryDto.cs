using LinkPara.Epin.Application.Commons.Mappings;
using LinkPara.Epin.Domain.Entities;
using LinkPara.Epin.Domain.Enums;

namespace LinkPara.Epin.Application.Features.Reconciliations;

public class ReconciliationSummaryDto: IMapFrom<ReconciliationSummary>
{
    public Guid Id { get; set; }
    public DateTime ReconciliationDate { get; set; }
    public decimal ExternalTotal { get; set; }
    public decimal OrderTotal { get; set; }
    public int ExternalCount { get; set; }
    public int OrderCount { get; set; }
    public string Message { get; set; }
    public ReconciliationStatus ReconciliationStatus { get; set; }
    public Organization Organization { get; set; }
    public List<ReconciliationDetailDto> UnreconciledOrders { get; set; }
}

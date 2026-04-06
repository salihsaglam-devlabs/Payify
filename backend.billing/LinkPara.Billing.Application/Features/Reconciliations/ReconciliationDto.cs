using LinkPara.Billing.Application.Commons.Mappings;
using LinkPara.Billing.Application.Commons.Models.Reconciliation;
using LinkPara.Billing.Domain.Enums;

namespace LinkPara.Billing.Application.Features.Reconciliations;

public class ReconciliationDto : IMapFrom<Reconciliation>
{
    public Guid VendorId { get; set; }
    public string VendorName { get; set; }
    public int TotalPaymentCount { get; set; }
    public decimal TotalPaymentAmount { get; set; }
    public int TotalCancelCount { get; set; }
    public decimal TotalCancelAmount { get; set; }
    public DateTime ReconciliationDate { get; set; }
    public ReconciliationStatus ReconciliationStatus { get; set; }
}
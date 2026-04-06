using LinkPara.Billing.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Billing.Domain.Entities;

public class Summary : AuditEntity
{
    public Guid VendorId { get; set; }
    public Vendor Vendor { get; set; }
    public int TotalPaymentCount { get; set; }
    public decimal TotalPaymentAmount { get; set; }
    public int TotalCancelCount { get; set; }
    public decimal TotalCancelAmount { get; set; }
    public int VendorTotalPaymentCount { get; set; }
    public decimal VendorTotalPaymentAmount { get; set; }
    public int VendorTotalCancelCount { get; set; }
    public decimal VendorTotalCancelAmount { get; set; }
    public DateTime ReconciliationDate { get; set; }
    public ReconciliationStatus ReconciliationStatus { get; set; }
    public string Explanation { get; set; }
}
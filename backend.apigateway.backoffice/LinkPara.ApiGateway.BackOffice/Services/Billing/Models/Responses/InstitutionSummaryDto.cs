using LinkPara.ApiGateway.BackOffice.Services.Billing.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Responses;

public class InstitutionSummaryDto
{
    public Guid Id { get; set; }
    public Guid VendorId { get; set; }
    public string VendorName { get; set; }
    public string SectorName { get; set; }
    public Guid InstitutionId { get; set; }
    public string InstitutionName { get; set; }
    public ReconciliationStatus ReconciliationStatus { get; set; }
    public DateTime ReconciliationDate { get; set; }
    public int TotalPaymentCount { get; set; }
    public int TotalCancelCount { get; set; }
    public decimal TotalPaymentAmount { get; set; }
    public decimal TotalCancelAmount { get; set; }
    public int VendorTotalPaymentCount { get; set; }
    public int VendorTotalCancelCount { get; set; }
    public decimal VendorTotalPaymentAmount { get; set; }
    public decimal VendorTotalCancelAmount { get; set; }
    public bool CanRetryReconciliation { get; set; }
    public string Explanation { get; set; }
}
using LinkPara.Billing.Domain.Enums;

namespace LinkPara.Billing.Application.Features.Reconciliations;

public class RetryReconciliationInstitutionResponseDto
{
    public string VendorName { get; set; }
    public string InstitutionName { get; set; }
    public string Description { get; set; }
    public DateTime ReconciliationDate { get; set; }
    public ReconciliationStatus ReconciliationStatus { get; set; }
}
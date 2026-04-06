using LinkPara.ApiGateway.BackOffice.Services.Billing.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Responses;

public class RetryReconciliationInstitutionResponseDto
{
    public string VendorName { get; set; }
    public string InstitutionName { get; set; }
    public string Description { get; set; }
    public DateTime ReconciliationDate { get; set; }
    public ReconciliationStatus ReconciliationStatus { get; set; }
}
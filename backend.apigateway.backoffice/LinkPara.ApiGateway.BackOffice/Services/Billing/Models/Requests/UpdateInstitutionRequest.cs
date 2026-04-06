using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Requests;

public class UpdateInstitutionRequest
{
    public Guid InstitutionId { get; set; }
    public Guid ActiveVendorId { get; set; }
    public RecordStatus RecordStatus { get; set; }
}
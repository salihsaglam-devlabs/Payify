using LinkPara.ApiGateway.BackOffice.Services.Billing.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Requests;

public class InstitutionFilterRequest : SearchQueryParams
{
    public Guid? VendorId { get; set; }
    public Guid? SectorId { get; set; }
    public OperationMode? OperationMode { get; set; }
    public RecordStatus? RecordStatus { get; set; }
}
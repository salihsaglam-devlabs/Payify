using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Billing.Models.Requests;

public class InstitutionFilterRequest : SearchQueryParams
{
    public RecordStatus? RecordStatus { get; set; }
    public Guid? SectorId { get; set; }
}
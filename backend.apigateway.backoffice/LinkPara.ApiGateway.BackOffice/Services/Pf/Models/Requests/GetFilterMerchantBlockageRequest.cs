using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests; 
public class GetFilterMerchantBlockageRequest : SearchQueryParams
{
    public MerchantBlockageStatus? MerchantBlockageStatus { get; set; }

    public Guid? MerchantId { get; set; }
}

using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;

public class GetFilterMerchantContentRequest : SearchQueryParams
{
    public Guid? MerchantId { get; set; }
    public MerchantContentSource? ContentSource { get; set; }
    public string Name { get; set; }
}
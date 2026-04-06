using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;

public interface IPricingProfileHttpClient
{
    Task<PaginatedList<PricingProfileDto>> GetFilterListAsync(GetFilterPricingProfileRequest request);
}

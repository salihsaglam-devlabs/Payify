using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;

public class PricingProfileHttpClient : HttpClientBase, IPricingProfileHttpClient
{
    public PricingProfileHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {

    }

    public async Task<PaginatedList<PricingProfileDto>> GetFilterListAsync(GetFilterPricingProfileRequest request)
    {
        var url = CreateUrlWithParams($"v1/PricingProfiles", request, true);
        var response = await GetAsync(url);
        var pricingProfiles = await response.Content.ReadFromJsonAsync<PaginatedList<PricingProfileDto>>();
        return pricingProfiles ?? throw new InvalidOperationException();
    }
}

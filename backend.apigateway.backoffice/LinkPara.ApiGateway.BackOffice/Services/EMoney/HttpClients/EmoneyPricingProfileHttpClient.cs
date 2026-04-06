using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;

public class EmoneyPricingProfileHttpClient : HttpClientBase, IEmoneyPricingProfileHttpClient
{
    public EmoneyPricingProfileHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) 
        : base(client, httpContextAccessor)
    {
    }

    public async Task DeleteAsync(Guid id)
    {
        var response = await DeleteAsync($"v1/PricingProfiles/{id}");
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task<EmoneyPricingProfileDto> GetByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/PricingProfiles/{id}");
        var pricingProfile = await response.Content.ReadFromJsonAsync<EmoneyPricingProfileDto>();
        return pricingProfile ?? throw new InvalidOperationException();
    }

    public async Task<PaginatedList<EmoneyPricingProfileDto>> GetListAsync(GetPricingProfileListRequest request)
    {
        var url = CreateUrlWithParams($"v1/PricingProfiles", request, true);
        var response = await GetAsync(url);
        var pricingProfiles = await response.Content.ReadFromJsonAsync<PaginatedList<EmoneyPricingProfileDto>>();
        return pricingProfiles ?? throw new InvalidOperationException();
    }

    public async Task SaveAsync(EmoneySavePricingProfileRequest request)
    {
        var response = await PostAsJsonAsync($"v1/PricingProfiles", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task UpdateAsync(EmoneyUpdatePricingProfileRequest request)
    {
        var response = await PutAsJsonAsync($"v1/PricingProfiles", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task UpdateProfileItemAsync(PricingProfileItemUpdateModel request)
    {
        var response = await PutAsJsonAsync($"v1/PricingProfiles/update-item", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }
}

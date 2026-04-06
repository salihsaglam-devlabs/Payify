using LinkPara.ApiGateway.Services.CampaignManagement.Models.Responses;

namespace LinkPara.ApiGateway.Services.CampaignManagement.HttpClients;

public class IWalletLocationtHttpClient : HttpClientBase, IIWalletLocationtHttpClient
{
    public IWalletLocationtHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
    {
    }

    public async Task<List<IWalletCityResponse>> GetCitiesAsync()
    {
        var response = await GetAsync($"v1/IWalletLocations/cities");

        return await response.Content.ReadFromJsonAsync<List<IWalletCityResponse>>();
    }

    public async Task<List<IWalletTownResponse>> GetTownsAsync(int cityId)
    {
        var response = await GetAsync($"v1/IWalletLocations/cities/{cityId}/towns");

        return await response.Content.ReadFromJsonAsync<List<IWalletTownResponse>>();
    }
}

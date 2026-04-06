using LinkPara.ApiGateway.Services.CampaignManagement.Models.Responses;
using LinkPara.ApiGateway.Services.Epin.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Http;

namespace LinkPara.ApiGateway.Services.CampaignManagement.HttpClients;

public class CampaingHttpClient : HttpClientBase, ICampaignHttpClient
{
    public CampaingHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
    {
    }

    public async Task<List<CampaignResponse>> GetCampaignsAsync()
    {
        var response = await GetAsync($"v1/Campaigns");

        return await response.Content.ReadFromJsonAsync<List<CampaignResponse>>();
    }
}

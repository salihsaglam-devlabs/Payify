using LinkPara.ApiGateway.Services.CampaignManagement.Models.Responses;

namespace LinkPara.ApiGateway.Services.CampaignManagement.HttpClients;

public class IWalletAgreementHttpClient : HttpClientBase, IIWalletAgreementHttpClient
{
    public IWalletAgreementHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
    {
    }

    public async Task<List<IWalletAgreementResponse>> GetAgreementsAsync()
    {
        var response = await GetAsync($"v1/IWalletAgreements");

        return await response.Content.ReadFromJsonAsync<List<IWalletAgreementResponse>>();
    }
}

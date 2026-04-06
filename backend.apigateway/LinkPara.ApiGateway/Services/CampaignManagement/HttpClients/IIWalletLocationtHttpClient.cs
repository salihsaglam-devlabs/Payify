using LinkPara.ApiGateway.Services.CampaignManagement.Models.Responses;

namespace LinkPara.ApiGateway.Services.CampaignManagement.HttpClients
{
    public interface IIWalletLocationtHttpClient
    {
        Task<List<IWalletCityResponse>> GetCitiesAsync();
        Task<List<IWalletTownResponse>> GetTownsAsync(int cityId);
    }
}

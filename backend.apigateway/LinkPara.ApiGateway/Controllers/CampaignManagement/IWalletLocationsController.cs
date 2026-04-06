using LinkPara.ApiGateway.Services.CampaignManagement.HttpClients;
using LinkPara.ApiGateway.Services.CampaignManagement.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Controllers.CampaignManagement;

public class IWalletLocationsController : ApiControllerBase
{
    private readonly IIWalletLocationtHttpClient _httpClient;

    public IWalletLocationsController(IIWalletLocationtHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Get IWallet Cities
    /// </summary>
    /// <returns></returns>
    [HttpGet("cities")]
    [Authorize(Policy = "IWalletLocation:ReadAll")]
    public async Task<List<IWalletCityResponse>> GetCitiesAsync()
    {
        return await _httpClient.GetCitiesAsync();
    }

    /// <summary>
    /// Get IWallet towns
    /// </summary>
    /// <returns></returns>
    [HttpGet("cities/{cityId}/towns")]
    [Authorize(Policy = "IWalletLocation:ReadAll")]
    public async Task<List<IWalletTownResponse>> GetTownsAsync([FromRoute]int cityId)
    {
        return await _httpClient.GetTownsAsync(cityId);
    }
}

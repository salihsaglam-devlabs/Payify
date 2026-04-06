using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.HttpClients;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.CorporateWallet.Controllers.Emoney;

public class TierLevelsController : ApiControllerBase
{
    private readonly ITierLevelHttpClient _httpClient;

    public TierLevelsController(ITierLevelHttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    /// <summary>
    /// Returns Tier levels
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "Limit:ReadAll")]
    [HttpGet("")]
    public async Task<List<TierLevelDto>> GetTierLevelsAsync()
    {
        return await _httpClient.GetTierLevelsAsync();
    }
}
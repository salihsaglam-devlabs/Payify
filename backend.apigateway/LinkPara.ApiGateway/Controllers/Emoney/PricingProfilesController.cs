using LinkPara.ApiGateway.Services.Emoney.HttpClients;
using LinkPara.ApiGateway.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Services.Emoney.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Controllers.Emoney;

public class PricingProfilesController : ApiControllerBase
{
    private readonly IPricingProfileHttpClient _pricingProfileHttpClient;

    public PricingProfilesController(IPricingProfileHttpClient pricingProfileHttpClient)
    {
        _pricingProfileHttpClient = pricingProfileHttpClient;
    }

    /// <summary>
    /// Returns commission rate of active profile item.
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyPricingProfiles:Read")]
    [HttpGet("")]
    public async Task<PricingProfileDto> GetCardTopupCommissionAsync()
    {
        return await _pricingProfileHttpClient.GetCardTopupCommissionAsync();
    }
}
using LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Merchant.Controllers.Pf;

public class PricingProfilesController : ApiControllerBase
{
    private readonly IPricingProfileHttpClient _pricingProfileHttpClient;

    public PricingProfilesController(IPricingProfileHttpClient pricingProfileHttpClient)
    {
        _pricingProfileHttpClient = pricingProfileHttpClient;
    }

    /// <summary>
    /// Returns filtered pricing profiles with items
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "PricingProfile:ReadAll")]
    public async Task<ActionResult<PaginatedList<PricingProfileDto>>> GetFilterAsync(
        [FromQuery] GetFilterPricingProfileRequest request)
    {
        return await _pricingProfileHttpClient.GetFilterListAsync(request);
    }
}
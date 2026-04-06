using LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Emoney;

public class EmoneyPricingProfilesController : ApiControllerBase
{
    private readonly IEmoneyPricingProfileHttpClient _pricingProfileHttpClient;

    public EmoneyPricingProfilesController(IEmoneyPricingProfileHttpClient pricingProfileHttpClient)
    {
        _pricingProfileHttpClient = pricingProfileHttpClient;
    }

    /// <summary>
    /// Returns all profiles.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "EmoneyPricingProfiles:ReadAll")]
    public async Task<ActionResult<PaginatedList<EmoneyPricingProfileDto>>> GetAllAsync([FromQuery] GetPricingProfileListRequest request)
    {
        return await _pricingProfileHttpClient.GetListAsync(request);
    }

    /// <summary>
    /// Returns a profile.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "EmoneyPricingProfiles:Read")]
    public async Task<ActionResult<EmoneyPricingProfileDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await _pricingProfileHttpClient.GetByIdAsync(id);
    }

    /// <summary>
    /// Creates a new profile with items
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("")]
    [Authorize(Policy = "EmoneyPricingProfiles:Create")]
    public async Task SaveAsync(EmoneySavePricingProfileRequest request)
    {
        await _pricingProfileHttpClient.SaveAsync(request);
    }

    /// <summary>
    /// Updates profile and its items.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("")]
    [Authorize(Policy = "EmoneyPricingProfiles:Update")]
    public async Task UpdateAsync(EmoneyUpdatePricingProfileRequest request)
    {
        await _pricingProfileHttpClient.UpdateAsync(request);
    }

    /// <summary>
    /// Updates a profile item.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("update-item")]
    [Authorize(Policy = "EmoneyPricingProfiles:Update")]
    public async Task UpdateItemAsync(PricingProfileItemUpdateModel request)
    {
        await _pricingProfileHttpClient.UpdateProfileItemAsync(request);
    }

    /// <summary>
    /// Deletes profile and its items.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("")]
    [Authorize(Policy = "EmoneyPricingProfiles:Delete")]
    public async Task DeleteAsync(Guid id)
    {
        await _pricingProfileHttpClient.DeleteAsync(id);
    }
}

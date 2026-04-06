using LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Pf;

public class PricingProfilesController : ApiControllerBase
{
    private readonly IPricingProfileHttpClient _pricingProfileHttpClient;

    public PricingProfilesController(IPricingProfileHttpClient pricingProfileHttpClient)
    {
        _pricingProfileHttpClient = pricingProfileHttpClient;
    }

    /// <summary>
    /// Returns a pricing profile with items
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "PricingProfile:Read")]
    public async Task<ActionResult<PricingProfileDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await _pricingProfileHttpClient.GetByIdAsync(id);
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

    /// <summary>
    /// Creates a new pricing profile with item
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("")]
    [Authorize(Policy = "PricingProfile:Create")]
    public async Task SaveAsync(SavePricingProfileRequest request)
    {
        await _pricingProfileHttpClient.SaveAsync(request);
    }

    /// <summary>
    /// Updates pricing profile with items
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("")]
    [Authorize(Policy = "PricingProfile:Update")]
    public async Task UpdateAsync(UpdatePricingProfileRequest request)
    {
        await _pricingProfileHttpClient.UpdateAsync(request);
    }

    /// <summary>
    /// Delete price profile
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("")]
    [Authorize(Policy = "PricingProfile:Delete")]
    public async Task DeleteAsync(Guid id)
    {
        await _pricingProfileHttpClient.DeleteProfileAsync(id);
    }

    /// <summary>
    /// Updates pricing profile with patch
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPatch("{id}")]
    [Authorize(Policy = "PricingProfile:Update")]
    public async Task PatchAsync(Guid id, [FromBody] JsonPatchDocument<UpdatePricingProfileRequest> request)
    {
        await _pricingProfileHttpClient.PatchAsync(id, request);
    }
    
    /// <summary>
    /// Checks loss-making rates in pricing profile update
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "PricingProfile:Update")]
    [HttpPost("update-preview")]
    public async Task<PricingProfilePreviewResponse> PreviewPricingProfileUpdateAsync(UpdatePreviewPricingProfileRequest request)
    {
        return await _pricingProfileHttpClient.PreviewPricingProfileUpdateAsync(request);
    }
}
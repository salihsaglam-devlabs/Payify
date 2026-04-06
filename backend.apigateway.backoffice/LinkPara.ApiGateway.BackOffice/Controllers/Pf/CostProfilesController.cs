using LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Pf;

public class CostProfilesController : ApiControllerBase
{
    private readonly ICostProfileHttpClient _costProfileHttpClient;

    public CostProfilesController(ICostProfileHttpClient costProfileHttpClient)
    {
        _costProfileHttpClient = costProfileHttpClient;
    }

    /// <summary>
    /// Returns a cost profile with items 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "CostProfile:Read")]
    public async Task<ActionResult<CostProfilesDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await _costProfileHttpClient.GetByIdAsync(id);
    }

    /// <summary>
    /// Returns filtered cost profiles with items
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "CostProfile:ReadAll")]
    public async Task<ActionResult<PaginatedList<CostProfilesDto>>> GetFilterAsync(
       [FromQuery] GetFilterCostProfileRequest request)
    {
        return await _costProfileHttpClient.GetFilterListAsync(request);
    }

    /// <summary>
    /// Creates a new cost profile with item
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("")]
    [Authorize(Policy = "CostProfile:Create")]
    public async Task SaveAsync(SaveCostProfileRequest request)
    {
        await _costProfileHttpClient.SaveAsync(request);
    }

    /// <summary>
    /// Updates cost profile with patch
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPatch("{id}")]
    [Authorize(Policy = "CostProfile:Update")]
    public async Task PatchAsync(Guid id, [FromBody] JsonPatchDocument<UpdateCostProfileRequest> request)
    {
        await _costProfileHttpClient.PatchAsync(id, request);
    }
    
    /// <summary>
    /// Checks loss-making rates in cost profile update
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "CostProfile:Update")]
    [HttpPost("update-preview")]
    public async Task<CostProfilePreviewResponse> PreviewCostProfileUpdateAsync(UpdatePreviewCostProfileRequest request)
    {
        return await _costProfileHttpClient.PreviewCostProfileUpdateAsync(request);
    }
}

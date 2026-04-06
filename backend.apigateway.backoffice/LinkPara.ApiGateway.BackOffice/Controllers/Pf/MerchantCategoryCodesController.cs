using LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Pf;

public class MerchantCategoryCodesController : ApiControllerBase
{
    private readonly IMccHttpClient _mccHttpClient;

    public MerchantCategoryCodesController(IMccHttpClient mccHttpClient)
    {
        _mccHttpClient = mccHttpClient;
    }

    /// <summary>
    /// Returns all mcc.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "MerchantCategoryCode:ReadAll")]
    public async Task<ActionResult<PaginatedList<MccDto>>> GetAllAsync([FromQuery] GetFilterMccRequest request)
    {
        return await _mccHttpClient.GetAllAsync(request);
    }

    /// <summary>
    /// Returns a mcc.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "MerchantCategoryCode:Read")]
    public async Task<ActionResult<MccDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await _mccHttpClient.GetByIdAsync(id);
    }

    /// <summary>
    /// Returns a mcc by code.
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    [HttpGet("{code}")]
    [Authorize(Policy = "MerchantCategoryCode:Read")]
    public async Task<ActionResult<MccDto>> GetByCodeAsync([FromRoute] string code)
    {
        return await _mccHttpClient.GetByCodeAsync(code);
    }

    /// <summary>
    /// Creates a mcc.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("")]
    [Authorize(Policy = "MerchantCategoryCode:Create")]
    public async Task SaveAsync(SaveMccRequest request)
    {
        await _mccHttpClient.SaveAsync(request);
    }

    /// <summary>
    /// Updates a mcc.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("")]
    [Authorize(Policy = "MerchantCategoryCode:Update")]
    public async Task UpdateAsync(UpdateMccRequest request)
    {
        await _mccHttpClient.UpdateAsync(request);
    }

    /// <summary>
    /// Delete a mcc.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("")]
    [Authorize(Policy = "MerchantCategoryCode:Delete")]
    public async Task DeleteAsync(Guid id)
    {
        await _mccHttpClient.DeleteMccAsync(id);
    }

    /// <summary>
    /// Updates mcc with patch
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPatch("{id}")]
    [Authorize(Policy = "MerchantCategoryCode:Update")]
    public async Task PatchAsync(Guid id, [FromBody] JsonPatchDocument<UpdateMccRequest> request)
    {
        await _mccHttpClient.PatchAsync(id, request);
    }
}

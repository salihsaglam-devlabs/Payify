using DocumentFormat.OpenXml.Office2010.Excel;
using LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Pf;

public class VposController : ApiControllerBase
{
    private readonly IVposHttpClient _vposHttpClient;

    public VposController(IVposHttpClient vposHttpClient)
    {
        _vposHttpClient = vposHttpClient;
    }

    /// <summary>
    /// Returns a Vpos with cost profile
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "Vpos:Read")]
    public async Task<ActionResult<VposDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await _vposHttpClient.GetByIdAsync(id);
    }

    /// <summary>
    /// Returns filtered Vpos with cost profile
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "Vpos:ReadAll")]
    public async Task<ActionResult<PaginatedList<VposDto>>> GetFilterAsync(
        [FromQuery] GetFilterVposRequest request)
    {
        return await _vposHttpClient.GetFilterListAsync(request);
    }

    /// <summary>
    /// Creates a new vpos with cost profile
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("")]
    [Authorize(Policy = "Vpos:Create")]
    public async Task SaveAsync(SaveVposRequest request)
    {
        await _vposHttpClient.SaveAsync(request);
    }

    /// <summary>
    /// Updates vpos with cost profile
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("")]
    [Authorize(Policy = "Vpos:Update")]
    public async Task UpdateAsync(UpdateVposRequest request)
    {
        await _vposHttpClient.UpdateAsync(request);
    }

    /// <summary>
    /// Delete vpos
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("")]
    [Authorize(Policy = "Vpos:Delete")]
    public async Task DeleteAsync(Guid id)
    {
        await _vposHttpClient.DeleteVposAsync(id);
    }

    /// <summary>
    /// Updates vpos with patch
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPatch("{id}")]
    [Authorize(Policy = "Vpos:Update")]
    public async Task PatchAsync(Guid id, [FromBody] JsonPatchDocument<PatchVposRequest> request)
    {
        await _vposHttpClient.PatchAsync(id, request);
    }

    /// <summary>
    /// Returns a Vpos with cost profile
    /// </summary>
    /// <param name="bkmReferenceNumber"></param>
    /// <returns></returns>
    [Authorize(Policy = "Vpos:Read")]
    [HttpGet("bkm/{bkmReferenceNumber}")]
    public async Task<ActionResult<MerchantVposDto>> GetByReferenceNumberAsync([FromRoute] string bkmReferenceNumber)
    {
        return await _vposHttpClient.GetByReferenceNumberAsync(bkmReferenceNumber);
    }
}

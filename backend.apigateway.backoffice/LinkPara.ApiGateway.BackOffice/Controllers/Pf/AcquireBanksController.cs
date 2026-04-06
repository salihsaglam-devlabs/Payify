using LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Pf;

public class AcquireBanksController : ApiControllerBase
{
    private readonly IAcquireBankHttpClient _acquireBankHttpClient;

    public AcquireBanksController(IAcquireBankHttpClient acquireBankHttpClient)
    {
        _acquireBankHttpClient = acquireBankHttpClient;
    }

    /// <summary>
    /// Returns all acquire banks.
    /// </summary>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "AcquireBank:ReadAll")]
    public async Task<ActionResult<PaginatedList<AcquireBankDto>>> GetAllAsync([FromQuery] GetFilterAcquireBankRequest request)
    {
        return await _acquireBankHttpClient.GetAllAsync(request);
    }

    /// <summary>
    /// Returns an acquire bank.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "AcquireBank:Read")]
    public async Task<ActionResult<AcquireBankDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await _acquireBankHttpClient.GetByIdAsync(id);
    }

    /// <summary>
    /// Creates an acquire bank.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("")]
    [Authorize(Policy = "AcquireBank:Create")]
    public async Task SaveAsync(SaveAcquireBankRequest request)
    {
        await _acquireBankHttpClient.SaveAsync(request);
    }

    /// <summary>
    /// Updates an acquire bank.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("")]
    [Authorize(Policy = "AcquireBank:Update")]
    public async Task UpdateAsync(UpdateAcquireBankRequest request)
    {
        await _acquireBankHttpClient.UpdateAsync(request);
    }

    /// <summary>
    /// Delete an acquire bank.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("")]
    [Authorize(Policy = "AcquireBank:Delete")]
    public async Task DeleteAsync(Guid id)
    {
        await _acquireBankHttpClient.DeleteAcquireBankAsync(id);
    }

    /// <summary>
    /// Updates acquire bank with patch
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPatch("{id}")]
    [Authorize(Policy = "AcquireBank:Update")]
    public async Task PatchAsync(Guid id, [FromBody] JsonPatchDocument<UpdateAcquireBankRequest> request)
    {
        await _acquireBankHttpClient.PatchAsync(id, request);
    }
}

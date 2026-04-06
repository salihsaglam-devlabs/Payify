using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests.Branch;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.MoneyTransfer;

public class BranchesController : ApiControllerBase
{
    private readonly IBranchHttpClient _client;

    public BranchesController(IBranchHttpClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Get All Branches
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Branches:ReadAll")]
    [HttpGet("")]
    public async Task<PaginatedList<BranchDto>> GetListAsync([FromQuery] GetBranchesRequest request)
    {
        return await _client.GetListAsync(request);
    }

    /// <summary>
    /// Creates New Branch
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Branches:Create")]
    [HttpPost("")]
    public async Task SaveAsync(SaveBranchRequest request)
    {
        await _client.SaveAsync(request);
    }

    [Authorize(Policy = "Branches:Update")]
    [HttpPatch("{id}")]
    public async Task PatchAsync(Guid id, [FromBody] JsonPatchDocument<UpdateBranchRequest> request)
    {
        await _client.PatchAsync(id, request);
    }
}
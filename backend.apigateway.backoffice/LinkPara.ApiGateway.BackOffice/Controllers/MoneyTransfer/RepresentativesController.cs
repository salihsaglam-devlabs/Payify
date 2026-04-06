using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests.Representative;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.MoneyTransfer;

public class RepresentativesController : ApiControllerBase
{
    private readonly IRepresentativeHttpClient _client;

    public RepresentativesController(IRepresentativeHttpClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Get All Representatives
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Representatives:ReadAll")]
    [HttpGet("")]
    public async Task<PaginatedList<RepresentativeDto>> GetListAsync([FromQuery]GetRepresentativesRequest request)
    {
        return await _client.GetListAsync(request);
    }

    /// <summary>
    /// Creates New Representative
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Representatives:Create")]
    [HttpPost("")]
    public async Task SaveAsync(SaveRepresentativeRequest request)
    {
        await _client.SaveAsync(request);
    }

    [Authorize(Policy = "Representatives:Update")]
    [HttpPatch("{id}")]
    public async Task PatchAsync(Guid id, [FromBody] JsonPatchDocument<UpdateRepresentativeRequest> request)
    {
        await _client.PatchAsync(id, request);
    }
}
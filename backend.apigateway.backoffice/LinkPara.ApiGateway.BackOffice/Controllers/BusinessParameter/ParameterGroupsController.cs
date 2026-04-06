using LinkPara.ApiGateway.BackOffice.Services.BusinessParameter.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.BusinessParameter.Models;
using LinkPara.ApiGateway.BackOffice.Services.BusinessParameter.Models.Request;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.BusinessParameter;

public class ParameterGroupsController : ApiControllerBase
{
    private readonly IParameterGroupHttpClient _parameterGroupHttpClient;

    public ParameterGroupsController(IParameterGroupHttpClient parameterGroupHttpClient)
    {
        _parameterGroupHttpClient = parameterGroupHttpClient;
    }

    /// <summary>
    /// Creates a parameter group
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("")]
    [Authorize(Policy = "ParameterGroup:Create")]
    public async Task SaveAsync(SaveParameterGroupDto request)
    {
        await _parameterGroupHttpClient.SaveAsync(request);
    }

    [HttpGet("")]
    [Authorize(Policy = "ParameterGroup:ReadAll")]
    public async Task<ActionResult<PaginatedList<ParameterGroupDto>>> GetAllAsync([FromQuery] GetAllParameterGroupRequest request)
    {
        return await _parameterGroupHttpClient.GetParameterGroupsAsync(request);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "ParameterGroup:Read")]
    public async Task<ActionResult<ParameterGroupDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await _parameterGroupHttpClient.GetParameterGroupByIdAsync(id);
    }

    /// <summary>
    /// Updates a parameter group
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("")]
    [Authorize(Policy = "ParameterGroup:Update")]
    public async Task<ParameterGroupDto> UpdateAsync(UpdateParameterGroupRequest request)
    {
        var parameterGroupResponse = await _parameterGroupHttpClient.UpdateAsync(request);

        return parameterGroupResponse;
    }

    /// <summary>
    /// Delete parameter group
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("")]
    [Authorize(Policy = "ParameterGroup:Delete")]
    public async Task DeleteAsync(Guid id)
    {
        await _parameterGroupHttpClient.DeleteParameterGroupAsync(id);
    }

}

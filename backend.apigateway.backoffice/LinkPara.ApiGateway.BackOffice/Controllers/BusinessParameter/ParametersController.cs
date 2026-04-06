using LinkPara.ApiGateway.BackOffice.Services.BusinessParameter.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.BusinessParameter.Models;
using LinkPara.ApiGateway.BackOffice.Services.BusinessParameter.Models.Request;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.BusinessParameter;

public class ParametersController : ApiControllerBase
{
    private readonly IParameterHttpClient _parameterHttpClient;

    public ParametersController(IParameterHttpClient parameterHttpClient)
    {
        _parameterHttpClient = parameterHttpClient;
    }

    [HttpGet("{groupCode}")]
    [Authorize(Policy = "Parameter:ReadAll")]
    public async Task<ActionResult<List<ParameterDto>>> GetAllAsync([FromRoute] string groupCode)
    {
        return await _parameterHttpClient.GetParametersAsync(groupCode);
    }

    [HttpGet("")]
    [Authorize(Policy = "Parameter:ReadAll")]
    public async Task<ActionResult<PaginatedList<ParameterDto>>> GetAllAsync([FromQuery] GetAllParameterRequest request)
    {
        return await _parameterHttpClient.GetAllParameterAsync(request);
    }

    /// <summary>
    /// Creates a parameter
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("")]
    [Authorize(Policy = "Parameter:Create")]
    public async Task SaveAsync(SaveParameterDto request)
    {
        await _parameterHttpClient.SaveAsync(request);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "Parameter:Read")]
    public async Task<ActionResult<ParameterDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await _parameterHttpClient.GetParameterByIdAsync(id);
    }

    /// <summary>
    /// Delete parameter
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("")]
    [Authorize(Policy = "Parameter:Delete")]
    public async Task DeleteAsync(Guid id)
    {
        await _parameterHttpClient.DeleteParameterAsync(id);
    }

    /// <summary>
    /// Updates a parameter
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("")]
    [Authorize(Policy = "Parameter:Update")]
    public async Task<ParameterDto> UpdateAsync(UpdateParameterRequest request)
    {
        var parameterResponse = await _parameterHttpClient.UpdateAsync(request);

        return parameterResponse;
    }
}

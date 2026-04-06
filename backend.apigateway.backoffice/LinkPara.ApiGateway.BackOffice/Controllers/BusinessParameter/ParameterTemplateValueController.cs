using LinkPara.ApiGateway.BackOffice.Services.BusinessParameter.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.BusinessParameter.Models;
using LinkPara.ApiGateway.BackOffice.Services.BusinessParameter.Models.Request;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.BusinessParameter;

public class ParameterTemplateValueController : ApiControllerBase
{
    private readonly IParameterTemplateValueHttpClient _parameterTemplateValueHttpClient;
    public ParameterTemplateValueController(IParameterTemplateValueHttpClient parameterTemplateValueHttpClient)
    {
        _parameterTemplateValueHttpClient = parameterTemplateValueHttpClient;
    }

    /// <summary>
    /// Creates a parameter template value
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "ParameterTemplateValue:Create")]
    [HttpPost("")]
    public async Task SaveAsync(SaveParameterTemplateValueDto request)
    {
        await _parameterTemplateValueHttpClient.SaveAsync(request);
    }

    [Authorize(Policy = "ParameterTemplateValue:Read")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ParameterTemplateValueDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await _parameterTemplateValueHttpClient.GetParameterTemplateValueByIdAsync(id);
    }

    /// <summary>
    /// Updates a parameter template value
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "ParameterTemplateValue:Update")]
    [HttpPut("")]
    public async Task<ParameterTemplateValueDto> UpdateAsync(UpdateParameterTemplateValueRequest request)
    {
        var parameterTemplateValueResponse = await _parameterTemplateValueHttpClient.UpdateAsync(request);

        return parameterTemplateValueResponse;
    }

    /// <summary>
    /// Delete parameter template value
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "ParameterTemplateValue:Delete")]
    [HttpDelete("")]
    public async Task DeleteAsync(Guid id)
    {
        await _parameterTemplateValueHttpClient.DeleteParameterTemplateValueAsync(id);
    }

    [Authorize(Policy = "ParameterTemplateValue:ReadAll")]
    [HttpGet("")]
    public async Task<ActionResult<PaginatedList<ParameterTemplateValueDto>>> GetAllAsync([FromQuery] GetAllParameterTemplateValueRequest request)
    {
        return await _parameterTemplateValueHttpClient.GetParameterTemplateValuesAsync(request);
    }

}

using LinkPara.ApiGateway.BackOffice.Services.BusinessParameter.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.BusinessParameter.Models;
using LinkPara.ApiGateway.BackOffice.Services.BusinessParameter.Models.Request;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.BusinessParameter;

public class ParameterTemplateController : ApiControllerBase
{
    private readonly IParameterTemplateHttpClient _parameterTemplateHttpClient;

    public ParameterTemplateController(IParameterTemplateHttpClient parameterTemplateHttpClient)
    {
        _parameterTemplateHttpClient = parameterTemplateHttpClient;
    }

    /// <summary>
    /// Creates a parameter template
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("")]
    [Authorize(Policy = "ParameterTemplate:Create")]
    public async Task SaveAsync(SaveParameterTemplateDto request)
    {
        await _parameterTemplateHttpClient.SaveAsync(request);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "ParameterTemplate:Read")]
    public async Task<ActionResult<ParameterTemplateDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await _parameterTemplateHttpClient.GetParameterTemplateByIdAsync(id);
    }

    /// <summary>
    /// Updates a parameter template
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("")]
    [Authorize(Policy = "ParameterTemplate:Update")]
    public async Task<ParameterTemplateDto> UpdateAsync(UpdateParameterTemplateRequest request)
    {
        var parameterTemplateResponse = await _parameterTemplateHttpClient.UpdateAsync(request);

        return parameterTemplateResponse;
    }

    /// <summary>
    /// Delete parameter template
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("")]
    [Authorize(Policy = "ParameterTemplate:Delete")]
    public async Task DeleteAsync(Guid id)
    {
        await _parameterTemplateHttpClient.DeleteParameterTemplateAsync(id);
    }

    [HttpGet("")]
    [Authorize(Policy = "ParameterTemplate:ReadAll")]
    public async Task<ActionResult<PaginatedList<ParameterTemplateDto>>> GetAllAsync([FromQuery] GetAllParameterTemplateRequest request)
    {
        return await _parameterTemplateHttpClient.GetParameterTemplatesAsync(request);
    }

    [HttpGet("{groupCode}")]
    [Authorize(Policy = "ParameterTemplate:ReadAll")]
    public async Task<ActionResult<List<ParameterTemplateDto>>> GetAllAsync([FromRoute] string groupCode)
    {
        return await _parameterTemplateHttpClient.GetParameterTemplatesByGroupCodeAsync(groupCode);
    }

}

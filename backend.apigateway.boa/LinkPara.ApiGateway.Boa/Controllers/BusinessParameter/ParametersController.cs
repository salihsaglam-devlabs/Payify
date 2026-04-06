using LinkPara.ApiGateway.Boa.Services.BusinessParameter.HttpClients;
using LinkPara.ApiGateway.Boa.Services.BusinessParameter.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Boa.Controllers.BusinessParameter;

public class ParametersController : ApiControllerBase
{
    private readonly IParameterHttpClient _parameterHttpClient;

    public ParametersController(IParameterHttpClient parameterHttpClient)
    {
        _parameterHttpClient = parameterHttpClient;
    }

    /// <summary>
    /// Returns all parameters of the group.
    /// </summary>
    /// <param name="groupCode"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("{groupCode}")]
    public async Task<ActionResult<List<ParameterDto>>> GetAllAsync([FromRoute] string groupCode)
    {
        return await _parameterHttpClient.GetParametersAsync(groupCode);
    }
}
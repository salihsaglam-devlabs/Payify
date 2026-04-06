using LinkPara.ApiGateway.CorporateWallet.Services.BusinessParameter.HttpClients;
using LinkPara.ApiGateway.CorporateWallet.Services.BusinessParameter.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.CorporateWallet.Controllers.BusinessParameter;

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
}

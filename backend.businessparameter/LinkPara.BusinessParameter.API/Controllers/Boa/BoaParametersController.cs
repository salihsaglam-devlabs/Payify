using LinkPara.BusinessParameter.Application.Features.Parameters;
using LinkPara.BusinessParameter.Application.Features.Parameters.Queries.GetAllParameter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.BusinessParameter.API.Controllers.Boa;

public class BoaParametersController : ApiControllerBase
{
    /// <summary>
    /// Returns all parameters by group code for the boa system.
    /// </summary>
    /// <param name="groupCode"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("{groupCode}")]
    public async Task<List<ParameterDto>> GetAllParametersByGroupCodeAsync([FromRoute] string groupCode)
    {
        return await Mediator.Send(new GetAllParameterByGroupCodeQuery { GroupCode = groupCode });
    }
}
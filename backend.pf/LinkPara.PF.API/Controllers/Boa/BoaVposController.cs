using LinkPara.PF.Application.Features.VirtualPos;
using LinkPara.PF.Application.Features.VirtualPos.Queries.GetFilterVpos;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.PF.API.Controllers.Boa;

public class BoaVposController : ApiControllerBase
{
    /// <summary>
    /// Returns filtered Vpos with cost profile
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("")]
    public async Task<ActionResult<PaginatedList<VposDto>>> GetFilterAsync([FromQuery] GetFilterVposQuery query)
    {
        return await Mediator.Send(query);
    }
}
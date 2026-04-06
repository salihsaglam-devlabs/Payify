using LinkPara.PF.Application.Features.NaceCodes;
using LinkPara.PF.Application.Features.NaceCodes.Queries.GetAllNaceCodes;
using LinkPara.PF.Application.Features.NaceCodes.Queries.GetNaceCodeById;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.PF.API.Controllers;

public class NaceCodesController : ApiControllerBase
{
    /// <summary>
    /// Returns all nace codes.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "NaceCode:ReadAll")]
    [HttpGet("")]
    public async Task<PaginatedList<NaceDto>> GetAllAsync([FromQuery] GetAllNaceCodesQuery request)
    {
        return await Mediator.Send(request);
    }
    
    /// <summary>
    /// Returns all nace codes.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "NaceCode:Read")]
    [HttpGet("{id}")]
    public async Task<NaceDto> GetAllAsync([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetNaceCodeById{ NaceCodeId = id});
    }
}
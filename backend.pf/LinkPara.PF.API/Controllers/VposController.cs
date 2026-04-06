using LinkPara.PF.Application.Commons.Models.Merchants;
using LinkPara.PF.Application.Commons.Models.VposModels.Request;
using LinkPara.PF.Application.Features.VirtualPos;
using LinkPara.PF.Application.Features.VirtualPos.Command.DeleteVpos;
using LinkPara.PF.Application.Features.VirtualPos.Command.PatchVpos;
using LinkPara.PF.Application.Features.VirtualPos.Command.SaveVpos;
using LinkPara.PF.Application.Features.VirtualPos.Command.UpdateVpos;
using LinkPara.PF.Application.Features.VirtualPos.Queries.GetFilterVpos;
using LinkPara.PF.Application.Features.VirtualPos.Queries.GetVposById;
using LinkPara.PF.Application.Features.VirtualPos.Queries.GetVposStatusByReferenceNumber;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.PF.API.Controllers;

public class VposController : ApiControllerBase
{
    /// <summary>
    /// Returns a Vpos with cost profile
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "Vpos:Read")]
    [HttpGet("{id}")]
    public async Task<ActionResult<VposDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetVposByIdQuery { Id = id });
    }

    /// <summary>
    /// Returns filtered Vpos with cost profile
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "Vpos:ReadAll")]
    [HttpGet("")]
    public async Task<ActionResult<PaginatedList<VposDto>>> GetFilterAsync([FromQuery] GetFilterVposQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Creates a new vpos with cost profile
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "Vpos:Create")]
    [HttpPost("")]
    public async Task SaveAsync(SaveVposCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Updates vpos with cost profile
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "Vpos:Update")]
    [HttpPut("")]
    public async Task UpdateAsync(UpdateVposCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Delete vpos
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "Vpos:Delete")]
    [HttpDelete("{id}")]
    public async Task DeleteAsync(Guid id)
    {
        await Mediator.Send(new DeleteVposCommand { Id = id });
    }

    /// <summary>
    /// Updates vpos with patch
    /// </summary>
    /// <param name="id"></param>
    /// <param name="vpos"></param>
    /// <returns></returns>
    [Authorize(Policy = "Vpos:Update")]
    [HttpPatch("{id}")]
    public async Task<UpdateVposRequest> PatchAsync(Guid id, [FromBody] JsonPatchDocument<UpdateVposRequest> vpos)
    {
        return await Mediator.Send(new PatchVposCommand { Id = id, Vpos = vpos });
    }


    /// <summary>
    /// Returns a Vpos with cost profile
    /// </summary>
    /// <param name="bkmReferenceNumber"></param>
    /// <returns></returns>
    [Authorize(Policy = "Vpos:Read")]
    [HttpGet("bkm/{bkmReferenceNumber}")]
    public async Task<ActionResult<MerchantVposDto>> GetByReferenceNumberAsync([FromRoute] string bkmReferenceNumber)
    {
        return await Mediator.Send(new GetVposStatusByReferenceNumberQuery { BkmReferenceNumber = bkmReferenceNumber });
    }
}

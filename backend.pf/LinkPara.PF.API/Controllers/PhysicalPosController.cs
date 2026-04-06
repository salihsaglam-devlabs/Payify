using LinkPara.PF.Application.Features.MerchantPhysicalDevices;
using LinkPara.PF.Application.Features.PhysicalPoses;
using LinkPara.PF.Application.Features.PhysicalPoses.Command.DeletePhysicalPos;
using LinkPara.PF.Application.Features.PhysicalPoses.Command.SavePhysicalPos;
using LinkPara.PF.Application.Features.PhysicalPoses.Command.UpdatePhysicalPos;
using LinkPara.PF.Application.Features.PhysicalPoses.Queries.GetAllPhysicalPos;
using LinkPara.PF.Application.Features.PhysicalPoses.Queries.GetPhysicalPosById;
using LinkPara.PF.Application.Features.PhysicalPoses.Queries.GetPhysicalPosStatusByReferenceNumber;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.PF.API.Controllers;

public class PhysicalPosController : ApiControllerBase
{
    /// <summary>
    /// Returns a PhysicalPos with cost profile
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "PhysicalPos:Read")]
    [HttpGet("{id}")]
    public async Task<ActionResult<PhysicalPosDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetPhysicalPosByIdQuery { Id = id });
    }

    /// <summary>
    /// Returns filtered PhysicalPos with cost profile
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "PhysicalPos:ReadAll")]
    [HttpGet("")]
    public async Task<ActionResult<PaginatedList<PhysicalPosDto>>> GetFilterAsync([FromQuery] GetAllPhysicalPosQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Creates a new PhysicalPos with cost profile
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "PhysicalPos:Create")]
    [HttpPost("")]
    public async Task SaveAsync(SavePhysicalPosCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Updates PhysicalPos with cost profile
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "PhysicalPos:Update")]
    [HttpPut("")]
    public async Task UpdateAsync(UpdatePhysicalPosCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Delete PhysicalPos
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "PhysicalPos:Delete")]
    [HttpDelete("{id}")]
    public async Task DeleteAsync(Guid id)
    {
        await Mediator.Send(new DeletePhysicalPosCommand { Id = id });
    }

    /// <summary>
    /// Returns a PhysicalPos 
    /// </summary>
    /// <param name="bkmReferenceNumber"></param>
    /// <returns></returns>
    [Authorize(Policy = "PhysicalPos:Read")]
    [HttpGet("bkm/{bkmReferenceNumber}")]
    public async Task<ActionResult<MerchantPhysicalPosDto>> GetByReferenceNumberAsync([FromRoute] string bkmReferenceNumber)
    {
        return await Mediator.Send(new GetPhysicalPosStatusByReferenceNumberQuery { BkmReferenceNumber = bkmReferenceNumber });
    }
}

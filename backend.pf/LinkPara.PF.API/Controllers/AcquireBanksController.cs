using LinkPara.PF.Application.Commons.Models.AcquireBanks;
using LinkPara.PF.Application.Features.AcquireBanks;
using LinkPara.PF.Application.Features.AcquireBanks.Command.DeleteAcquireBank;
using LinkPara.PF.Application.Features.AcquireBanks.Command.PatchAcquireBank;
using LinkPara.PF.Application.Features.AcquireBanks.Command.SaveAcquireBank;
using LinkPara.PF.Application.Features.AcquireBanks.Command.UpdateAcquireBank;
using LinkPara.PF.Application.Features.AcquireBanks.Queries.GetAcquireBankById;
using LinkPara.PF.Application.Features.AcquireBanks.Queries.GetAllAcquireBank;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.PF.API.Controllers;

public class AcquireBanksController : ApiControllerBase
{
    /// <summary>
    /// Returns all acquire banks.
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "AcquireBank:ReadAll")]
    [HttpGet("")]
    public async Task<PaginatedList<AcquireBankDto>> GetAllAsync([FromQuery] GetAllAcquireBankQuery request)
    {
        return await Mediator.Send(request);
    }

    /// <summary>
    /// Returns an acquire bank.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "AcquireBank:Read")]
    [HttpGet("{id}")]
    public async Task<ActionResult<AcquireBankDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetAcquireBankByIdQuery { Id = id });
    }

    /// <summary>
    /// Creates an acquire bank.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "AcquireBank:Create")]
    [HttpPost("")]
    public async Task SaveAsync(SaveAcquireBankCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Updates an acquire bank.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "AcquireBank:Update")]
    [HttpPut("")]
    public async Task UpdateAsync(UpdateAcquireBankCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Delete an acquire bank.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "AcquireBank:Delete")]
    [HttpDelete("{id}")]
    public async Task DeleteAsync(Guid id)
    {
        await Mediator.Send(new DeleteAcquireBankCommand { Id = id});
    }

    /// <summary>
    /// Updates acquire bank with patch
    /// </summary>
    /// <param name="id"></param>
    /// <param name="acquireBank"></param>
    /// <returns></returns>
    [Authorize(Policy = "AcquireBank:Delete")]
    [HttpPatch("{id}")]
    public async Task<UpdateAcquireBankRequest> Patch(Guid id, [FromBody] JsonPatchDocument<UpdateAcquireBankRequest> acquireBank)
    {
        return await Mediator.Send(new PatchAcquireBankCommand { Id = id, AcquireBank = acquireBank });
    }
}

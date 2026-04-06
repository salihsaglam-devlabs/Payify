using LinkPara.PF.Application.Features.BankLimits;
using LinkPara.PF.Application.Features.BankLimits.Queries.GetAllBankLimit;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LinkPara.PF.Application.Features.BankLimits.Queries.GetBankLimitById;
using LinkPara.PF.Application.Features.BankLimits.Command.SaveBankLimit;
using LinkPara.PF.Application.Features.BankLimits.Command.DeleteBankLimit;
using LinkPara.PF.Application.Features.BankLimits.Command.UpdateBankLimit;

namespace LinkPara.PF.API.Controllers;
public class BankLimitsController : ApiControllerBase
{
    /// <summary>
    /// Returns all bank limits.
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "BankLimit:ReadAll")]
    [HttpGet("")]
    public async Task<PaginatedList<BankLimitDto>> GetAllAsync([FromQuery] GetAllBankLimitQuery request)
    {
        return await Mediator.Send(request);
    }

    /// <summary>
    /// Returns an bank limit.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "BankLimit:Read")]
    [HttpGet("{id}")]
    public async Task<ActionResult<BankLimitDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetBankLimitByIdQuery { Id = id });
    }

    /// <summary>
    /// Creates an bank limit.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "BankLimit:Create")]
    [HttpPost("")]
    public async Task SaveAsync(SaveBankLimitCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Updates an bank limit.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "BankLimit:Update")]
    [HttpPut("")]
    public async Task UpdateAsync(UpdateBankLimitCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Delete an bank limit.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "BankLimit:Delete")]
    [HttpDelete("{id}")]
    public async Task DeleteAsync(Guid id)
    {
        await Mediator.Send(new DeleteBankLimitCommand { Id = id });
    }
}

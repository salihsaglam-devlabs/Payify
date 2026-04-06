using LinkPara.PF.Application.Features.BankHealthChecks;
using LinkPara.PF.Application.Features.BankHealthChecks.Command.UpdateBankHealthCheck;
using LinkPara.PF.Application.Features.BankHealthChecks.Queries.GetAllBankHealthCheck;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.PF.API.Controllers;

public class BankHealthChecksController : ApiControllerBase
{
    /// <summary>
    /// Returns all health checks.
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "BankHealthCheck:ReadAll")]
    [HttpGet("")]
    public async Task<PaginatedList<BankHealthCheckDto>> GetAllAsync([FromQuery] GetAllBankHealthCheckQuery request)
    {
        return await Mediator.Send(request);
    }

    /// <summary>
    /// Updates an health check.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "BankHealthCheck:Update")]
    [HttpPut("")]
    public async Task UpdateAsync(UpdateBankHealthCheckCommand command)
    {
        await Mediator.Send(command);
    }
}

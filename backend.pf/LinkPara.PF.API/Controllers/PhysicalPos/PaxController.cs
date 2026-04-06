using LinkPara.PF.Application.Commons.Models.PhysicalPos.Response;
using LinkPara.PF.Application.Features.PhysicalPos.Pax.Command.PaxEndOfDayCommand;
using LinkPara.PF.Application.Features.PhysicalPos.Pax.Command.PaxParameterCommand;
using LinkPara.PF.Application.Features.PhysicalPos.Pax.Command.PaxReconciliationCommand;
using LinkPara.PF.Application.Features.PhysicalPos.Pax.Command.PaxTransactionCommand;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.PF.API.Controllers.PhysicalPos;

public class PaxController : ApiControllerBase
{
    /// <summary>
    /// Pax Device Transaction Request
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "PhysicalPos:Create")]
    [HttpPost("transaction")]
    public async Task<PaxTransactionResponse> PaxTransactionAsync(PaxTransactionCommand command)
    {
        return await Mediator.Send(command);
    }
    
    /// <summary>
    /// Pax Device Parameter Request
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "PhysicalPos:Create")]
    [HttpPost("parameter")]
    public async Task<PaxParameterResponse> PaxParameterAsync(PaxParameterCommand command)
    {
        return await Mediator.Send(command);
    }
    
    /// <summary>
    /// Pax Device End of Day Request
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "PhysicalPos:Create")]
    [HttpPost("eod")]
    public async Task<PaxEndOfDayResponse> PaxEndOfDayAsync(PaxEndOfDayCommand command)
    {
        return await Mediator.Send(command);
    }
    
    /// <summary>
    /// Pax Device Reconciliation Request
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "PhysicalPos:Create")]
    [HttpPost("reconciliation")]
    public async Task<PaxReconciliationResponse> PaxReconciliationAsync(PaxReconciliationCommand command)
    {
        return await Mediator.Send(command);
    }
}
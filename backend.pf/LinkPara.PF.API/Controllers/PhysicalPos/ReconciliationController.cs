using LinkPara.PF.Application.Features.PhysicalPos.Reconciliation;
using LinkPara.PF.Application.Features.PhysicalPos.Reconciliation.Command.BatchManualReconciliation;
using LinkPara.PF.Application.Features.PhysicalPos.Reconciliation.Command.SingleManualReconciliation;
using LinkPara.PF.Application.Features.PhysicalPos.Reconciliation.Queries.DownloadPhysicalPosEndOfDayDetailById;
using LinkPara.PF.Application.Features.PhysicalPos.Reconciliation.Queries.GetAllPhysicalPosEndOfDay;
using LinkPara.PF.Application.Features.PhysicalPos.Reconciliation.Queries.GetPhysicalPosEndOfDayDetailById;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.PF.API.Controllers.PhysicalPos;

public class ReconciliationController : ApiControllerBase
{
    /// <summary>
    /// Get all EndOfDay records
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "PhysicalPosReconciliation:ReadAll")]
    [HttpGet("")]
    public async Task<ActionResult<PaginatedList<PhysicalPosEndOfDayDto>>> GetFilterAsync([FromQuery] GetAllPhysicalPosEndOfDayQuery query)
    {
        return await Mediator.Send(query);
    }
    
    /// <summary>
    /// Get EndOfDay with all related transactions
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "PhysicalPosReconciliation:Read")]
    [HttpGet("{id}")]
    public async Task<ActionResult<PhysicalPosEndOfDayDetailResponse>> GetByIdAsync([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetPhysicalPosEndOfDayDetailByIdQuery { Id = id });
    }
    
    /// <summary>
    /// Download EndOfDay excel with all related transactions
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "PhysicalPosReconciliation:Read")]
    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadEndOfDayExcelAsync([FromRoute] Guid id)
    {
        return await Mediator.Send(new DownloadPhysicalPosEndOfDayDetailByIdQuery { Id = id });
    }
    
    /// <summary>
    /// Batch Manual Reconciliation
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "PhysicalPosReconciliation:Create")]
    [HttpPost("batch-manual-reconciliation")]
    public async Task BatchManualReconciliationAsync(BatchManualReconciliationCommand command)
    {
        await Mediator.Send(command);
    }
    
    /// <summary>
    /// Singular Manual Reconciliation
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "PhysicalPosReconciliation:Create")]
    [HttpPost("single-manual-reconciliation")]
    public async Task SingleManualReconciliationAsync(SingleManualReconciliationCommand command)
    {
        await Mediator.Send(command);
    }
}
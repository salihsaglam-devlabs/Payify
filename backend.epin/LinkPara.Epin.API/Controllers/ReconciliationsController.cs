using Microsoft.AspNetCore.Mvc;
using LinkPara.Epin.Application.Features.Reconciliations.Commands.ReconciliationByDate;
using LinkPara.SharedModels.Pagination;
using LinkPara.Epin.Application.Features.Reconciliations.Queries.GetFilterReconciliationSummaries;
using LinkPara.Epin.Application.Features.Reconciliations;
using LinkPara.Epin.Application.Features.Reconciliations.Queries.GetReconciliationSummaryById;
using Microsoft.AspNetCore.Authorization;

namespace LinkPara.Epin.API.Controllers;

public class ReconciliationsController : ApiControllerBase
{
    /// <summary>
    /// Reconciliation By Date
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "EpinReconciliation:Create")]
    [HttpPost("")]
    public async Task ReconciliationByDateAsync([FromBody] ReconciliationByDateCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Get Reconciliations Filter
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "EpinReconciliation:ReadAll")]
    [HttpGet("summary")]
    public async Task<PaginatedList<ReconciliationSummaryDto>> GetFilterReconciliationSummariesAsync([FromQuery] GetFilterReconciliationSummariesQuery request)
    {
        return await Mediator.Send(request);
    }

    /// <summary>
    /// Get Reconciliation Summary by Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "EpinReconciliation:Read")]
    [HttpGet("summary/{id}")]
    public async Task<ReconciliationSummaryDto> GetReconciliationSummaryByIdAsync([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetReconciliationSummaryByIdQuery { ReconciliationSummaryId = id});
    }
}

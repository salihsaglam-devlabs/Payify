using LinkPara.PF.Application.Features.SubMerchants;
using LinkPara.PF.Application.Features.SubMerchants.Queries.GetAllSubMerchant;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LinkPara.PF.Application.Features.SubMerchants.Queries.GetSubMerchantById;
using LinkPara.PF.Application.Features.SubMerchants.Command.SaveSubMerchant;
using LinkPara.PF.Application.Features.SubMerchants.Command.UpdateSubMerchant;
using LinkPara.PF.Application.Features.SubMerchants.Command.DeleteSubMerchant;
using LinkPara.PF.Application.Features.SubMerchants.Command.UpdateMultipleSubMerchant;
using LinkPara.PF.Application.Features.SubMerchants.Command.ApproveSubMerchant;
using LinkPara.PF.Application.Features.SubMerchants.Queries.GetSubMerchantSummary;

namespace LinkPara.PF.API.Controllers;

public class SubMerchantsController : ApiControllerBase
{
    /// <summary>
    /// Returns all sub merchants.
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "SubMerchant:ReadAll")]
    [HttpGet("")]
    public async Task<PaginatedList<SubMerchantDto>> GetAllAsync([FromQuery] GetAllSubMerchantQuery request)
    {
        return await Mediator.Send(request);
    }

    /// <summary>
    /// Returns a sub merchant.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "SubMerchant:Read")]
    [HttpGet("{id}")]
    public async Task<ActionResult<SubMerchantDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetSubMerchantByIdQuery { Id = id });
    }
    
    /// <summary>
    /// Returns a sub merchants summary.
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "SubMerchant:Read")]
    [HttpGet("summary")]
    public async Task<ActionResult<SubMerchantSummaryDto>> GetSummaryByUserIdAsync()
    {
        return await Mediator.Send(new GetSubMerchantSummaryQuery());
    }

    /// <summary>
    /// Creates a sub merchant.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "SubMerchant:Create")]
    [HttpPost("")]
    public async Task<Guid> SaveAsync(SaveSubMerchantCommand command)
    {
        return await Mediator.Send(command);
    }

    /// <summary>
    /// Updates a sub merchant.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "SubMerchant:Update")]
    [HttpPut("")]
    public async Task UpdateAsync(UpdateSubMerchantCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Approve/Reject a sub merchant.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "SubMerchant:Update")]
    [HttpPut("approve")]
    public async Task ApproveAsync(ApproveSubMerchantCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Update a sub merchants.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "SubMerchant:Update")]
    [HttpPut("multiple")]
    public async Task UpdateMultipleAsync(UpdateMultipleSubMerchantCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Delete a sub merchant.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "SubMerchant:Delete")]
    [HttpDelete("{id}")]
    public async Task DeleteAsync(Guid id)
    {
        await Mediator.Send(new DeleteSubMerchantCommand { Id = id });
    }
}

using LinkPara.PF.Application.Commons.Models.SubMerchants;
using LinkPara.PF.Application.Features.SubMerchantLimits.Commands.DeleteSubMerchantLimit;
using LinkPara.PF.Application.Features.SubMerchantLimits.Commands.SaveSubMerchantLimit;
using LinkPara.PF.Application.Features.SubMerchantLimits.Commands.UpdateSubMerchantLimit;
using LinkPara.PF.Application.Features.SubMerchantLimits.Queries.GetAllSubMerchantLimits;
using LinkPara.PF.Application.Features.SubMerchantLimits.Queries.GetSubMerchantLimitById;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.PF.API.Controllers;

public class SubMerchantLimitsController : ApiControllerBase
{
    /// <summary>
    /// Returns all sub merchant limits.
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "SubMerchantLimit:ReadAll")]
    [HttpGet("")]
    public async Task<PaginatedList<SubMerchantLimitDto>> GetAllAsync([FromQuery] GetAllSubMerchantLimitsQuery request)
    {
        return await Mediator.Send(request);
    }

    /// <summary>
    /// Returns a sub merchant limit.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "SubMerchantLimit:Read")]
    [HttpGet("{id}")]
    public async Task<ActionResult<SubMerchantLimitDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetSubMerchantLimitByIdQuery { SubMerchantLimitId = id });
    }

    /// <summary>
    /// Deletes a sub merchant limit.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "SubMerchantLimit:Delete")]
    [HttpDelete("{id:guid}")]
    public async Task DeleteAsync([FromRoute] Guid id)
    {
        await Mediator.Send(new DeleteSubMerchantLimitCommand { Id = id });
    }
    
    /// <summary>
    /// Creates a sub merchant Limit.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "SubMerchantLimit:Create")]
    [HttpPost("")]
    public async Task SaveAsync([FromBody] SaveSubMerchantLimitCommand request)
    {
        await Mediator.Send(request);
    }
    
    /// <summary>
    /// Updates a sub merchant Limit.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "SubMerchantLimit:Update")]
    [HttpPut("")]
    public async Task UpdateAsync([FromBody] SubMerchantLimitDto request)
    {
        await Mediator.Send(new UpdateSubMerchantLimitCommand{ SubMerchantLimit = request});
    }
}
using LinkPara.PF.Application.Commons.Models.Merchants;
using LinkPara.PF.Application.Features.MerchantPools;
using LinkPara.PF.Application.Features.MerchantPools.Command.ApproveMerchantPool;
using LinkPara.PF.Application.Features.MerchantPools.Command.SaveMerchantPool;
using LinkPara.PF.Application.Features.MerchantPools.Queries.GetFilterMerchantPool;
using LinkPara.PF.Application.Features.MerchantPools.Queries.GetMerchantPoolById;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.PF.API.Controllers;

public class MerchantPoolsController : ApiControllerBase
{
    /// <summary>
    /// Returns a merchant that make an application
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantPool:Read")]
    [HttpGet("{id}")]
    public async Task<ActionResult<MerchantPoolDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetMerchantPoolByIdQuery { Id = id });
    }

    /// <summary>
    /// Returns filtered merchant that make an application
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantPool:ReadAll")]
    [HttpGet("")]
    public async Task<ActionResult<PaginatedList<MerchantPoolDto>>> GetFilterAsync([FromQuery] GetFilterMerchantPoolQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Creates a merchant that make an application
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantPool:Create")]
    [HttpPost("")]
    public async Task SaveAsync(SaveMerchantPoolCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Performs the approval/rejection process for the merchant that make an application
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantPool:Update")]
    [HttpPut("")]
    public async Task<ApproveMerchantPoolResponse> Approve(ApproveMerchantPoolCommand command)
    {
        return await Mediator.Send(command);
    }
}

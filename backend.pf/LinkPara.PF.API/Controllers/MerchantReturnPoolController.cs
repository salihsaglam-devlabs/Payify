using LinkPara.PF.Application.Commons.Models.Payments.Response;
using LinkPara.PF.Application.Commons.Models.VposModels.Request;
using LinkPara.PF.Application.Features.MerchantReturnPools;
using LinkPara.PF.Application.Features.MerchantReturnPools.Command.ActionMerchantReturnPool;
using LinkPara.PF.Application.Features.MerchantReturnPools.Queries.GetMerchantReturnPools;
using LinkPara.PF.Application.Features.VirtualPos;
using LinkPara.PF.Application.Features.VirtualPos.Command.DeleteVpos;
using LinkPara.PF.Application.Features.VirtualPos.Command.PatchVpos;
using LinkPara.PF.Application.Features.VirtualPos.Command.SaveVpos;
using LinkPara.PF.Application.Features.VirtualPos.Command.UpdateVpos;
using LinkPara.PF.Application.Features.VirtualPos.Queries.GetFilterVpos;
using LinkPara.PF.Application.Features.VirtualPos.Queries.GetVposById;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.PF.API.Controllers;

public class MerchantReturnPoolController : ApiControllerBase
{

    /// <summary>
    /// Analyst takes action for merchant return pool
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantReturnPool:Update")]
    [HttpPost("")]
    public async Task<ReturnResponse> ActionMerchantReturnPoolAsync(ActionMerchantReturnPoolCommand command)
    {
        return await Mediator.Send(command);
    }

    /// <summary>
    /// Gets merchant return pool
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantReturnPool:ReadAll")]
    [HttpGet("")]
    public async Task<PaginatedList<MerchantReturnPoolDto>> GetMerchantReturnPoolAsync([FromQuery] GetMerchantReturnPoolsQuery command)
    {
        return await Mediator.Send(command);
    }
}

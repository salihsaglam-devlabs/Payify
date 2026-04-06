using LinkPara.PF.Application.Features.SubMerchantUsers;
using LinkPara.PF.Application.Features.SubMerchantUsers.Command.SaveSubMerchantUser;
using LinkPara.PF.Application.Features.SubMerchantUsers.Command.UpdateSubMerchantUser;
using LinkPara.PF.Application.Features.SubMerchantUsers.Queries.GetAllSubMerchantUsers;
using LinkPara.PF.Application.Features.SubMerchantUsers.Queries.GetSubMerchantUserById;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.PF.API.Controllers;

public class SubMerchantUsersController : ApiControllerBase
{
    /// <summary>
    /// Returns submerchant users
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "SubMerchantUser:ReadAll")]
    [HttpGet("")]
    public async Task<ActionResult<PaginatedList<SubMerchantUserDto>>> GetAllAsync([FromQuery] GetAllSubMerchantUserQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Returns a submerchant user
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "SubMerchantUser:Read")]
    [HttpGet("{id}")]
    public async Task<ActionResult<SubMerchantUserDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetSubMerchantUserByIdQuery { Id = id });
    }

    /// <summary>
    /// Create a submerchant user
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "SubMerchantUser:Create")]
    [HttpPost("")]
    public async Task SaveAsync(SaveSubMerchantUserCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Update a submerchant user
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "SubMerchantUser:Update")]
    [HttpPut("")]
    public async Task UpdateAsync(UpdateSubMerchantUserCommand command)
    {
        await Mediator.Send(command);
    }
}
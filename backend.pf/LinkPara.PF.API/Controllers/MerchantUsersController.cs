using LinkPara.PF.Application.Commons.Models.MerchantUser;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Mvc;
using LinkPara.PF.Application.Features.MerchantUsers.Queries.GetAllMerchantUser;
using LinkPara.PF.Application.Features.MerchantUsers;
using LinkPara.PF.Application.Features.MerchantUsers.Queries.GetMerchantUserById;
using LinkPara.PF.Application.Features.MerchantUsers.Command.SaveMerchantUser;
using LinkPara.PF.Application.Features.MerchantUsers.Command.UpdateMerchantUser;
using LinkPara.PF.Application.Features.MerchantUsers.Queries.GetUserName;
using Microsoft.AspNetCore.Authorization;

namespace LinkPara.PF.API.Controllers;

public class MerchantUsersController : ApiControllerBase
{
    /// <summary>
    /// Returns a merchant users
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantUser:ReadAll")]
    [HttpGet("")]
    public async Task<ActionResult<PaginatedList<MerchantUserDto>>> GetAllAsync([FromQuery] GetAllMerchantUserQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Returns a merchant user
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantUser:Read")]
    [HttpGet("{id}")]
    public async Task<ActionResult<MerchantUserDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetMerchantUserByIdQuery { Id = id });
    }

    /// <summary>
    /// Create a merchant user
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantUser:Create")]
    [HttpPost("")]
    public async Task SaveAsync(SaveMerchantUserCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Update a merchant user
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantUser:Update")]
    [HttpPut("")]
    public async Task UpdateAsync(UpdateMerchantUserCommand command)
    {
        await Mediator.Send(command);
    }
    
    /// <summary>
    /// Returns username
    /// </summary>
    /// <param name="identifier"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("username/{identifier}")]
    public async Task<ActionResult<MerchantUserNameModel>> GetUserNameAsync([FromRoute] string identifier)
    {
        return await Mediator.Send(new GetUserNameQuery { Identifier = identifier });
    }
}

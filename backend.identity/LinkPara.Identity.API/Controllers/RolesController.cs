using LinkPara.Identity.Application.Features.Roles.Commands.CreateRole;
using LinkPara.Identity.Application.Features.Roles.Commands.DeleteRole;
using LinkPara.Identity.Application.Features.Roles.Commands.UpdateRole;
using LinkPara.Identity.Application.Features.Roles.Queries;
using LinkPara.Identity.Application.Features.Roles.Queries.RoleFilter;
using LinkPara.Identity.Application.Features.Roles.Queries.GetRoleById;
using Microsoft.AspNetCore.Mvc;
using LinkPara.Identity.Application.Features.Users.Queries;
using LinkPara.Identity.Application.Features.Roles.Queries.GetUsersByRoleId;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;

namespace LinkPara.Identity.API.Controllers;

public class RolesController : ApiControllerBase {

    [Authorize(Policy = "Role:Read")]
    [HttpGet("{roleId}")]
    public async Task<ActionResult<RoleDetailDto>> GetByIdAsync(Guid roleId)
    {
        return await Mediator.Send(new GetRoleByIdQuery() { RoleId = roleId });
    }
    [Authorize(Policy = "Role:ReadAll")]
    [HttpGet("")]
    public async Task<PaginatedList<RoleDto>> GetRolesAsync([FromQuery] RoleFilterQuery query) {
        return await Mediator.Send(query);
    }
    [Authorize(Policy = "Role:Create")]
    [HttpPost("")]
    public async Task CreateAsync(CreateRoleCommand command)
    {
        await Mediator.Send(command);
    }
    [Authorize(Policy = "Role:Update")]
    [HttpPut("{roleId}")]
    public async Task UpdateAsync(UpdateRoleCommand command)
    {
        await Mediator.Send(command);
    }
    [Authorize(Policy = "Role:Delete")]
    [HttpDelete("{roleId}")]
    public async Task DeleteAsync(Guid roleId)
    {
        await Mediator.Send(new DeleteRoleCommand() { RoleId = roleId });
    }
    [Authorize(Policy = "Role:Read")]
    [HttpGet("{roleId}/users")]
    public async Task<ActionResult<List<UserDto>>> GetUsersByRoleIdAsync(Guid roleId)
    {
        return await Mediator.Send(new GetUsersByRoleIdQuery { RoleId = roleId });
    }
}
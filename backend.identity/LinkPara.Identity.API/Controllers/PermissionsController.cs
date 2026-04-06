using LinkPara.Identity.Application.Features.Permissions.Commands.SyncPermissions;
using LinkPara.Identity.Application.Features.Permissions.Commands.UpdatePermission;
using LinkPara.Identity.Application.Features.Users.Queries;
using LinkPara.Identity.Application.Features.Users.Queries.GetUserById;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Identity.API.Controllers;

public class PermissionsController : ApiControllerBase
{
    [Authorize(Policy = "Permission:ReadAll")]
    [HttpGet("")]
    public async Task<List<PermissionDto>> GetAllAsync()
    {
        return await Mediator.Send(new GetAllPermissionsQuery());
    }

    [Authorize(Policy = "Permission:Create")]
    [HttpPost("synchronise")]
    public async Task SyncPermissionsAsync(SyncPermissionsCommand command)
    {
        await Mediator.Send(command);
    }

    [Authorize(Policy = "Permission:Update")]
    [HttpPut("{permissionId}")]
    public async Task UpdateAsync([FromRoute] Guid permissionId, UpdatePermissionDto command)
    {
        await Mediator.Send(new UpdatePermissionCommand { Id = permissionId, UpdatePermission = command });
    }
}
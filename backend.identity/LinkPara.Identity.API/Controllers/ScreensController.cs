using LinkPara.Identity.Application.Features.Roles.Commands.DeleteRole;
using LinkPara.Identity.Application.Features.Screens;
using LinkPara.Identity.Application.Features.Screens.Commands.CreateRoleScreen;
using LinkPara.Identity.Application.Features.Screens.Commands.DeleteRoleScreen;
using LinkPara.Identity.Application.Features.Screens.Commands.UpdateRoleScreen;
using LinkPara.Identity.Application.Features.Screens.Queries;
using LinkPara.Identity.Application.Features.Screens.Queries.GetAllScreens;
using LinkPara.Identity.Application.Features.Screens.Queries.GetScreensByRoleId;
using LinkPara.Identity.Application.Features.Screens.Queries.GetScreensByUserId;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Identity.API.Controllers
{
    public class ScreensController : ApiControllerBase
    {
        [Authorize(Policy = "Role:ReadAll")]
        [HttpGet("")]
        public async Task<ActionResult<List<ScreenDto>>> GetAllScreensAsync()
        {
            return await Mediator.Send(new GetAllScreensQuery());
        }

        [Authorize(Policy = "Role:ReadAll")]
        [HttpGet("role-menu/{userId}")]
        public async Task<ActionResult<List<ScreenDto>>> GetScreensByUserIdAsync(Guid userId)
        {
            return await Mediator.Send(new GetScreensByUserIdQuery { UserId = userId });
        }

        [Authorize(Policy = "Role:ReadAll")]
        [HttpGet("{roleId}")]
        public async Task<ActionResult<RoleScreenDto>> GetScreensByRoleIdAsync(Guid roleId)
        {
            return await Mediator.Send(new GetScreensByRoleIdQuery { RoleId = roleId });
        }

        [Authorize(Policy = "Role:Update")]
        [HttpPut("{roleId}")]
        public async Task UpdateRoleWithScreensAsync(UpdateRoleScreenCommand command)
        {
            await Mediator.Send(command);
        }

        [Authorize(Policy = "Role:Create")]
        [HttpPost()]
        public async Task CreateRoleWithScreens(CreateRoleScreenCommand command)
        {
            await Mediator.Send(command);
        }

        [Authorize(Policy = "Role:Delete")]
        [HttpDelete("{roleId}")]
        public async Task DeleteAsync(Guid roleId)
        {
            await Mediator.Send(new DeleteRoleScreenCommand() { RoleId = roleId });
        }
    }
}

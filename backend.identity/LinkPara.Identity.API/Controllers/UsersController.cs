using LinkPara.Identity.Application.Common.Models.AccountModels;
using LinkPara.Identity.Application.Common.Models.AuthorizationModels;
using LinkPara.Identity.Application.Features.AgreementDocuments.Queries;
using LinkPara.Identity.Application.Features.AgreementDocuments.Queries.GetUserDocuments;
using LinkPara.Identity.Application.Features.Roles.Queries;
using LinkPara.Identity.Application.Features.Users.Commands.AssignUserRole;
using LinkPara.Identity.Application.Features.Users.Commands.CreateUser;
using LinkPara.Identity.Application.Features.Users.Commands.PatchUser;
using LinkPara.Identity.Application.Features.Users.Commands.UpdateUser;
using LinkPara.Identity.Application.Features.Users.Commands.VerifyEmail;
using LinkPara.Identity.Application.Features.Users.Queries;
using LinkPara.Identity.Application.Features.Users.Queries.AppUserFilter;
using LinkPara.Identity.Application.Features.Users.Queries.GetExistingUserList;
using LinkPara.Identity.Application.Features.Users.Queries.GetIsUserExist;
using LinkPara.Identity.Application.Features.Users.Queries.GetUserById;
using LinkPara.Identity.Application.Features.Users.Queries.GetUserClaims;
using LinkPara.Identity.Application.Features.Users.Queries.GetUserLoginActivity;
using LinkPara.Identity.Application.Features.Users.Queries.GetUserRole;
using LinkPara.Identity.Application.Features.Users.Queries.GetUsersByIds;
using LinkPara.Identity.Application.Features.Users.Queries.GetUserToken;
using LinkPara.Identity.Application.Features.Users.Queries.UserFilter;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Identity.API.Controllers;

public class UsersController : ApiControllerBase
{
    [Authorize(Policy = "User:Read")]
    [HttpGet("{userId}")]
    public async Task<ActionResult<UserDto>> GetByIdAsync(Guid userId)
    {
        return await Mediator.Send(new GetUserByIdQuery { UserId = userId });
    }

    [Authorize(Policy = "User:Read")]
    [HttpGet("username/{userName}")]
    public async Task<ActionResult<UserDto>> GetByUserNameAsync(string userName)
    {
        return await Mediator.Send(new GetUserByUserNameQuery { UserName = userName });
    }

    [AllowAnonymous]
    [HttpGet("user-exist/{userName}")]
    public async Task<ActionResult<bool>> GetIsUserExistAsync(string userName)
    {
        return await Mediator.Send(new GetIsUserExistQuery { UserName = userName });
    }

    [Authorize(Policy = "User:ReadAll")]
    [HttpGet("")]
    public async Task<PaginatedList<UserDtoWithRoles>> GetUsersAsync([FromQuery] UserFilterQuery query)
    {
        return await Mediator.Send(query);
    }

    [AllowAnonymous]
    [HttpGet("app-user")]
    public async Task<List<UserDto>> GetAppUsersAsync([FromQuery] AppUserFilterQuery query)
    {
        return await Mediator.Send(query);
    }
    
    [AllowAnonymous]
    [HttpGet("check-existing-users")]
    public async Task<ExistingUsersDto> GetExistingUserListAsync([FromQuery] GetExistingUserListQuery query)
    {
        return await Mediator.Send(query);
    }

    [Authorize(Policy = "User:ReadAll")]
    [HttpGet("all")]
    public async Task<PaginatedList<UserDto>> GetAllUsersAsync([FromQuery] AllUserFilterQuery query)
    {
        return await Mediator.Send(query);
    }

    [AllowAnonymous]
    [HttpPost("")]
    public async Task<ActionResult<UserCreateResponse>> CreateAsync(CreateUserCommand command)
    {
        return await Mediator.Send(command);
    }

    [Authorize(Policy = "User:Update")]
    [HttpPut("")]
    public async Task UpdateAsync(UpdateUserCommand command)
    {
        await Mediator.Send(command);
    }

    [Authorize(Policy = "User:Update")]
    [HttpPatch("{userId}")]
    public async Task<UserDto> PatchAsync(Guid userId, [FromBody] JsonPatchDocument<PatchUserDto> patchUserDto)
    {
        return await Mediator.Send(new PatchUserCommand { UserId = userId, PatchUserDto = patchUserDto });
    }

    [Authorize(Policy = "User:Update")]
    [HttpPost("{userId}/roles")]
    public async Task AddRoleToUserAsync(Guid userId, [FromBody] UserRoleDto command)
    {
        await Mediator.Send(new AssignUserRoleCommand { UserId = userId, Roles = command.Roles });
    }

    [Authorize(Policy = "User:ReadAll")]
    [HttpGet("{userId}/roles")]
    public async Task<ActionResult<List<RoleDto>>> GetUserRolesAsync(Guid userId)
    {
        return await Mediator.Send(new GetUserRolesQuery { UserId = userId });
    }

    [Authorize(Policy = "User:ReadAll")]
    [HttpGet("{userId}/claims")]
    public async Task<ActionResult<List<ClaimDto>>> GetUserClaimsAsync(Guid userId)
    {
        return await Mediator.Send(new GetUserClaimsQuery { UserId = userId });
    }

    [Authorize(Policy = "User:ReadAll")]
    [HttpGet("{userId}/agreementDocuments")]
    public async Task<ActionResult<List<UserAgreementDocumentsStatusDto>>> GetUserDocumentsAsync(Guid userId, bool getOptionalDocuments)
    {
        return await Mediator.Send(new GetUserDocumentsQuery() { UserId = userId, GetOptionalDocuments = getOptionalDocuments});
    }

    [Authorize(Policy = "User:Read")]
    [HttpGet("{userId}/token")]
    public async Task<ActionResult<TokenResponse>> GetUserTokenAsync(Guid userId, string secureKey)
    {
        return await Mediator.Send(new GetUserTokenQuery { UserId = userId, SecureKey = secureKey });
    }

    [Authorize(Policy = "User:Read")]
    [HttpGet("{userId}/loginActivity")]
    public async Task<ActionResult<GetUserLoginActivityResponse>> GetUserLoginActivity(Guid userId)
    {
        return await Mediator.Send(new GetUserLoginActivityQuery { UserId = userId });
    }

    [Authorize(Policy = "User:Read")]
    [HttpGet("{userId}/loginActivity/{channel}")]
    public async Task<ActionResult<GetUserLoginActivityResponse>> GetUserLoginActivityByChannelAsync(Guid userId, string channel)
    {
        return await Mediator.Send(new GetUserLoginActivityQuery { UserId = userId, Channel = channel });
    }

    [Authorize(Policy = "User:Update")]
    [HttpPut("{userId}/removeUserLock")]
    public async Task RemoveUserLockAsync(Guid userId)
    {
        await Mediator.Send(new RemoveUserLockCommand() { UserId = userId });
    }
    [AllowAnonymous]
    [HttpPost("VerifyEmail")]
    public async Task<bool> VerifyUserEmail(VerifyEmailCommand command)
    {
        return await Mediator.Send(command);
    }
    [Authorize(Policy ="User:Read")]
    [HttpPost("ResendEmailVerify")]
    public async Task<bool> ResendEmailVerify(ResendEmailVerifyCommand command)
    {
        return await Mediator.Send(command);
    }
    [Authorize(Policy = "User:ReadAll")]
    [HttpPost("search-by-ids")]
    public async Task<List<UserDto>> GetUsersByIds(GetUsersByIdsQuery query)
    {
        return await Mediator.Send(query);
    }
}
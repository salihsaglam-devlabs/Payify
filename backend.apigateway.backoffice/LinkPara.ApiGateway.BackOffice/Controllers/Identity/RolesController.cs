using LinkPara.ApiGateway.BackOffice.Services.Identity.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Enums;
using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Identity;

public class RolesController : ApiControllerBase
{
    private readonly IRoleHttpClient _roleHttpClient;

    public RolesController(IRoleHttpClient roleHttpClient)
    {
        _roleHttpClient = roleHttpClient;
    }

    /// <summary>
    /// Lists all created roles.
    /// </summary>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "Role:ReadAll")]
    public async Task<ActionResult<PaginatedList<RoleDto>>> GetAllAsync([FromQuery] GetRolesRequest request)
    {
        return await _roleHttpClient.GetAllRolesAsync(request);
    }


    /// <summary>
    /// Gets the role detail with claims.
    /// </summary>
    /// <param name="roleId"></param>
    /// <returns></returns>
    [HttpGet("{roleId}")]
    [Authorize(Policy = "Role:Read")]
    public async Task<ActionResult<RoleDetailDto>> GetRoleAsync(string roleId)
    {
        return await _roleHttpClient.GetRoleAsync(roleId);
    }

    /// <summary>
    /// Get users by roleId
    /// </summary>
    /// <param name="roleId"></param>
    /// <returns></returns>
    [HttpGet("{roleId}/users")]
    [Authorize(Policy = "Role:Read")]
    public async Task<ActionResult<List<UserDto>>> GetUsersByRoleIdAsync(Guid roleId)
    {
        return await _roleHttpClient.GetUsersByRoleIdAsync(roleId);
    }

    /// <summary>
    /// Creates a new role.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("")]
    [Authorize(Policy = "Role:Create")]
    public async Task CreateRoleAsync(RoleRequest request)
    {
        await _roleHttpClient.CreateRoleAsync(request);
    }

    /// <summary>
    /// Creates a new role.
    /// </summary>
    /// <param name="roleId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("{roleId}")]
    [Authorize(Policy = "Role:Update")]
    public async Task UpdateRoleAsync([FromRoute] string roleId, RoleRequest request)
    {
        await _roleHttpClient.UpdateRoleAsync(new UpdateRoleRequest()
        {
            RoleId = roleId,
            RoleScope = request.RoleScope,
            Name = request.Name
        });
    }

    /// <summary>
    /// Removes created role.
    /// </summary>
    /// <param name="roleId"></param>
    /// <returns></returns>
    [HttpDelete("{roleId}")]
    [Authorize(Policy = "Role:Delete")]
    public async Task DeleteRoleAsync(string roleId)
    {
        await _roleHttpClient.DeleteRoleAsync(roleId);
    }
}
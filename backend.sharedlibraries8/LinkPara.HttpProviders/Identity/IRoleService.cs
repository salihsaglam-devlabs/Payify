using LinkPara.HttpProviders.Identity.Models;

namespace LinkPara.HttpProviders.Identity;

public interface IRoleService
{
    Task<RoleDetailDto> GetRoleAsync(Guid roleId);
    Task<List<UserDto>> GetUsersByRoleIdAsync(Guid roleId);
}

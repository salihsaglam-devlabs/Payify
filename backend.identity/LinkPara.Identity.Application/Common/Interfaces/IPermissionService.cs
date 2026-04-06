using LinkPara.Identity.Application.Features.Users.Queries;
using LinkPara.Identity.Domain.Entities;

namespace LinkPara.Identity.Application.Common.Interfaces;

public interface IPermissionService
{
    Task AddRangePermissionAsync(List<Permission> permissions);

    Task UpdatePermissionAsync(Permission permission);

    Task<List<PermissionDto>> GetAllPermissionAsync();

    Task<Permission> GetPermissionByIdAsync(Guid id);

    Task<List<Permission>> GetPermissionOwnerModulesByIdAsync(Guid id);

    Task<bool> CheckInPermissionsAsync(string claimValue);
}
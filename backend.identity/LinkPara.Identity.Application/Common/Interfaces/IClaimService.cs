using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Authorization.Enums;
using System.Security.Claims;

namespace LinkPara.Identity.Application.Common.Interfaces;

public interface IClaimService
{
    Task AddClaimToRoleAsync(Role role, List<PermissionOperationType> operations, string module);

    Task RemoveClaimFromRoleAsync(Role role, List<Claim> claims = null);

    Task AddClaimToUserAsync(User user, List<PermissionOperationType> operations, string module);

    Task RemoveClaimFromUserAsync(User user, List<Claim> claims = null);

    Task RemoveClaimFromUserAsync(User user, List<PermissionOperationType> operations, string module);

    Task<IList<Claim>> GetRoleClaimsAsync(Role role);

    Task<IList<Claim>> GetUserClaimsAsync(User user);

    Task<bool> RoleClaimVerifyAsync(Role role, Claim claim);

    Task<bool> UserClaimVerifyAsync(User user, Claim claim);
}
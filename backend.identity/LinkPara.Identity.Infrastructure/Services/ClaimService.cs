using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.SharedModels.Authorization.Models;
using LinkPara.SharedModels.Authorization.Enums;
using LinkPara.Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;

namespace LinkPara.Identity.Infrastructure.Services;

public class ClaimService : IClaimService
{
    private readonly ILogger<ClaimService> _logger;
    private readonly RoleManager<Role> _roleManager;
    private readonly UserManager<User> _userManager;
    private readonly IPermissionService _permissionService;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IRepository<RoleClaim> _roleClaimRepository;

    public ClaimService(ILogger<ClaimService> logger,
        RoleManager<Role> roleManager,
        UserManager<User> userManager,
        IPermissionService permissionService,
        IApplicationUserService applicationUserService,
        IRepository<RoleClaim> roleClaimRepository)
    {
        _logger = logger;
        _roleManager = roleManager;
        _userManager = userManager;
        _permissionService = permissionService;
        _applicationUserService = applicationUserService;
        _roleClaimRepository = roleClaimRepository;
    }

    public async Task AddClaimToRoleAsync(Role role, List<PermissionOperationType> operations, string module)
    {
        var roleClaims = await _roleManager.GetClaimsAsync(role);

        foreach (var operation in operations)
        {
            var claimKeyValue = $"{module}:{operation}";

            if (roleClaims.All(q => q.Value != claimKeyValue) &&
                await _permissionService.CheckInPermissionsAsync(claimKeyValue))
            {
                var roleClaim = new RoleClaim
                {
                    RoleId = role.Id,
                    ClaimType = ClaimKey.RoleScope,
                    ClaimValue = claimKeyValue,
                    CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
                    RecordStatus = RecordStatus.Active
                };

                await _roleClaimRepository.AddAsync(roleClaim);
            }
        }
    }

    public async Task RemoveClaimFromRoleAsync(Role role, List<Claim> claims = null)
    {
        var roleClaims = claims ?? await _roleManager.GetClaimsAsync(role);

        if (roleClaims.Any())
        {
            foreach (var roleClaim in roleClaims)
            {
                var result = await _roleManager.RemoveClaimAsync(role, roleClaim);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"Removed claim \"{roleClaim.Value}\" from role \"{role.Name}\".");
                }
            }
        }
    }

    public async Task AddClaimToUserAsync(User user, List<PermissionOperationType> operations, string module)
    {
        var userClaims = await _userManager.GetClaimsAsync(user);

        foreach (var operation in operations)
        {
            var claimKeyValue = $"{module}:{operation}";

            if (!userClaims.Any(q => q.Value.ToUpperInvariant() == claimKeyValue.ToUpperInvariant()) &&
                await _permissionService.CheckInPermissionsAsync(claimKeyValue))
            {
                var result = await _userManager.AddClaimAsync(user, new Claim(ClaimKey.UserScope, claimKeyValue));

                if (result.Succeeded)
                {
                    _logger.LogInformation($"Added claim \"{claimKeyValue}\" to user \"{user.UserName}\".");
                }
            }
        }
    }

    public async Task RemoveClaimFromUserAsync(User user, List<PermissionOperationType> operations, string module)
    {
        var userClaims = await _userManager.GetClaimsAsync(user);

        foreach (var operation in operations)
        {
            var claimKeyValue = $"{module}:{operation}";

            var claim = userClaims.FirstOrDefault(q => q.Value.ToUpperInvariant() == claimKeyValue.ToUpperInvariant());

            if (claim is not null)
            {
                var result = await _userManager.RemoveClaimAsync(user, claim);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"Removed claim \"{claimKeyValue}\" from user \"{user.UserName}\".");
                }
            }
        }
    }

    public async Task RemoveClaimFromUserAsync(User user, List<Claim> claims = null)
    {
        var userClaims = claims ?? await _userManager.GetClaimsAsync(user);

        if (userClaims.Any())
        {
            foreach (var userClaim in userClaims)
            {
                var result = await _userManager.RemoveClaimAsync(user, userClaim);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"Removed claim \"{userClaim.Value}\" from user \"{user.Id}\".");
                }
            }
        }
    }

    public async Task<IList<Claim>> GetRoleClaimsAsync(Role role)
    {
        return await _roleManager.GetClaimsAsync(role);
    }

    public async Task<IList<Claim>> GetUserClaimsAsync(User user)
    {
        return await _userManager.GetClaimsAsync(user);
    }

    public async Task<bool> RoleClaimVerifyAsync(Role role, Claim claim)
    {
        var roleClaims = await _roleManager.GetClaimsAsync(role);

        var result = roleClaims.Any(q => q.Value == claim.Value);

        return result;
    }

    public async Task<bool> UserClaimVerifyAsync(User user, Claim claim)
    {
        var userClaims = await _userManager.GetClaimsAsync(user);

        var result = userClaims.Any(q => q.Value == claim.Value) || await CheckInUserRolesAsync(user, claim);

        return result;
    }

    private async Task<bool> CheckInUserRolesAsync(User user, Claim claim)
    {
        var userRoles = await _userManager.GetRolesAsync(user);

        foreach (var userRole in userRoles)
        {
            if (await RoleClaimVerifyAsync(await _roleManager.FindByNameAsync(userRole), claim))
            {
                return true;
            }
        }

        return false;
    }
}
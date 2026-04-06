using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;

using Microsoft.EntityFrameworkCore;
using LinkPara.SharedModels.Persistence;
using LinkPara.Identity.Application.Common.Models.AuthorizationModels;
using LinkPara.SharedModels.Authorization.Enums;
using LinkPara.Identity.Domain.Enums;
using LinkPara.Audit.Models;
using LinkPara.Audit;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Data;
using System.Security.Claims;
using LinkPara.ContextProvider;

namespace LinkPara.Identity.Application.Features.Screens.Commands.UpdateRoleScreen
{
    public class UpdateRoleScreenCommand : IRequest
    {
        public string RoleId { get; set; }
        public Guid[] ScreensId { get; set; }
        public string Name { get; set; }
        public bool CanSeeSensitiveData { get; set; }
        public RoleScope RoleScope { get; set; }

    }

    public class UpdateRoleScreenCommandHandler : IRequestHandler<UpdateRoleScreenCommand>
    {
        private readonly RoleManager<Role> _roleManager;
        private readonly IRepository<Screen> _screenRepository;
        private readonly IRepository<RoleScreen> _roleScreenRepository;
        private readonly IRepository<ScreenClaim> _screenClaim;
        private readonly IClaimService _claimService;
        private readonly IAuditLogService _auditLogService;
        private readonly IContextProvider _contextProvider;
        private readonly IRepository<Permission> _permissionRepository;
        private readonly IRepository<RoleClaim> _roleClaimRepository;
        private readonly IScreenService _screenService;

        public UpdateRoleScreenCommandHandler(RoleManager<Role> roleManager,
            IRepository<Screen> screenRepository,
            IRepository<RoleScreen> roleScreenRepository,
            IRepository<ScreenClaim> screenClaim,
            IClaimService claimService,
            IRepository<Permission> permissionRepository,
            IRepository<RoleClaim> roleClaimRepository,
            IAuditLogService auditLogService,
            IContextProvider contextProvider,
            IScreenService screenService)
        {
            _roleManager = roleManager;
            _screenRepository = screenRepository;
            _roleScreenRepository = roleScreenRepository;
            _screenClaim = screenClaim;
            _claimService = claimService;
            _permissionRepository = permissionRepository;
            _roleClaimRepository = roleClaimRepository;
            _auditLogService = auditLogService;
            _contextProvider = contextProvider;
            _screenService = screenService;
        }

        public async Task<Unit> Handle(UpdateRoleScreenCommand command, CancellationToken cancellationToken)
        {
            var role = await _roleManager.FindByIdAsync(command.RoleId);

            var userId = _contextProvider.CurrentContext.UserId;
            var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

            if (role is null)
            {
                throw new NotFoundException(nameof(Role), command.RoleId);
            }
            await UpdateRoleScreenAsync(command, parseUserId);

            await UpdateRoleAsync(role, command, parseUserId);

            var matchingScreenClaims = await GetMatchRoleScreenClaims(command);

            var roleClaims = await GetRoleClaimsForDisplayTrue(command);

            foreach (var roleClaim in roleClaims)
            {
                await _roleManager.RemoveClaimAsync(role, roleClaim);
            }

            foreach (var matchingClaim in matchingScreenClaims)
            {
                await _claimService.AddClaimToRoleAsync(role, new List<PermissionOperationType> { matchingClaim.OperationType }, matchingClaim.Module);
            }


            await CheckRoleClaimForDisplayFalse(command, role);

            await CheckExistingDashboardScreen(command, role, parseUserId);

            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = true,
                    LogDate = DateTime.Now,
                    Operation = "UpdateRole",
                    SourceApplication = "Identity",
                    Resource = "Role",
                    UserId = parseUserId,
                    Details = new Dictionary<string, string>
                    {
                    {"Name", command.Name }
                    }
                });
            return Unit.Value;
        }
        private async Task CheckExistingDashboardScreen(UpdateRoleScreenCommand command,Role role, Guid userId)
        {
            var dashboardName = role.RoleScope == RoleScope.Internal ? "DASHBOARD.TITLE" : "MERCHANT.DASHBOARD";

            var existingDashboardScreen = await _roleScreenRepository.GetAll(rs => rs.Screen)
                .FirstOrDefaultAsync(rs => rs.RoleId == Guid.Parse(command.RoleId) &&
                                           rs.Screen.Name == dashboardName &&
                                           rs.Screen.OperationType == PermissionOperationType.ReadAll &&
                                           rs.RecordStatus == RecordStatus.Active);

            if (existingDashboardScreen is null)
            {
                var dashboardScreen = await _screenRepository.GetAll()
                    .FirstOrDefaultAsync(screen =>
                        screen.Name == dashboardName &&
                        screen.OperationType == PermissionOperationType.ReadAll &&
                        screen.RecordStatus == RecordStatus.Active);

                var roleScreen = new RoleScreen
                {
                    RoleId = Guid.Parse(command.RoleId),
                    ScreenId = dashboardScreen.Id,
                    CreatedBy = userId.ToString()
                };

                await _roleScreenRepository.AddAsync(roleScreen);
            }
        }
        private async Task CheckRoleClaimForDisplayFalse(UpdateRoleScreenCommand command, Role role)
        {
            var existingRoleClaims = await _roleClaimRepository.GetAll()
                .Where(rc => rc.RoleId == Guid.Parse(command.RoleId))
                .Select(rc => rc.ClaimValue)
                .ToListAsync();

            var newPermissionsToAdd = await _permissionRepository.GetAll()
                .Where(permission => permission.Display == false && !existingRoleClaims.Contains(permission.ClaimValue))
                .ToListAsync();

            if (newPermissionsToAdd.Any())
            {
                foreach (var permission in newPermissionsToAdd)
                {
                    ModuleClaim moduleClaim = new();
                    List<PermissionOperationType> permissionOperationTypes = new()
                {
                    permission.OperationType
                };
                    moduleClaim.PermissionOperationTypes = permissionOperationTypes;
                    moduleClaim.Module = permission.Module;
                    await _claimService.AddClaimToRoleAsync(role, moduleClaim.PermissionOperationTypes, moduleClaim.Module);
                }
            }
        }
        private async Task<List<Claim>> GetRoleClaimsForDisplayTrue(UpdateRoleScreenCommand command)
        {
            return await _roleClaimRepository.GetAll()
                .Where(rc => rc.RoleId == Guid.Parse(command.RoleId)) 
                .Join(
                    _permissionRepository.GetAll().Where(permission => permission.Display), 
                    roleClaim => roleClaim.ClaimValue,
                    permission => permission.ClaimValue,
                    (roleClaim, permission) => new Claim(roleClaim.ClaimType, roleClaim.ClaimValue))
                .ToListAsync();
        }
        private async Task<List<Permission>> GetMatchRoleScreenClaims(UpdateRoleScreenCommand command)
        {
            return await _screenClaim.GetAll()
                .Where(sc => command.ScreensId.Contains(sc.ScreenId))
                .Join(
                    _permissionRepository.GetAll(),
                    screenClaim => screenClaim.ClaimValue,
                    permission => permission.ClaimValue,
                    (screenClaim, permission) => permission)
                 .Distinct()
                .ToListAsync();
        }
        private async Task UpdateRoleScreenAsync(UpdateRoleScreenCommand command , Guid userId)
        {
            var existingRoleScreens = await _roleScreenRepository.GetAll(rs => rs.Screen)
                .Where(rs => rs.RoleId == Guid.Parse(command.RoleId) && rs.RecordStatus == RecordStatus.Active)
                .ToListAsync();

            var screensToAdd = command.ScreensId.Except(existingRoleScreens.Select(rs => rs.ScreenId)).ToList();

            foreach (var roleScreenToRemove in existingRoleScreens.Where(rs => !command.ScreensId.Contains(rs.ScreenId)).ToList())
            {
                await _screenService.DeleteRoleScreenAsync(roleScreenToRemove);
            }

            foreach (var screenIdToAdd in screensToAdd)
            {
                var screen = await _screenRepository.GetByIdAsync(screenIdToAdd);
                if (screen != null)
                {
                    var newRoleScreen = new RoleScreen
                    {
                        RoleId = Guid.Parse(command.RoleId),
                        ScreenId = screenIdToAdd,
                        CreatedBy = userId.ToString()
                    };
                    await _roleScreenRepository.AddAsync(newRoleScreen);
                }
            }
        }
        private async Task UpdateRoleAsync(Role role, UpdateRoleScreenCommand command,Guid userId)
        {
            if (role.Name != command.Name)
            {
                role.Name = command.Name;
            }
            if (role.RoleScope != command.RoleScope)
            {
                role.RoleScope = command.RoleScope;
            }
            if (role.CanSeeSensitiveData != command.CanSeeSensitiveData)
            {
                role.CanSeeSensitiveData = command.CanSeeSensitiveData;
            }

            role.UpdateDate = DateTime.Now;
            role.LastModifiedBy = userId.ToString();

            var updateResult = await _roleManager.UpdateAsync(role);

            if (!updateResult.Succeeded)
            {
                await _auditLogService.AuditLogAsync(
                    new AuditLog
                    {
                        IsSuccess = false,
                        LogDate = DateTime.Now,
                        Operation = "UpdateRole",
                        SourceApplication = "Identity",
                        Resource = "Role",
                        UserId = userId,
                        Details = new Dictionary<string, string>
                        {
                            {"Name", command.Name },
                            {"ErrorMessage" , updateResult.Errors.FirstOrDefault().Description}
                        }
                    });

                throw new InvalidDataException(updateResult.Errors.FirstOrDefault()?.Description);
            }
        }

    }
}

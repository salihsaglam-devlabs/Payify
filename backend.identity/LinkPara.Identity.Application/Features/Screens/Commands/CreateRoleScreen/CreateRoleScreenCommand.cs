using LinkPara.Audit.Models;
using LinkPara.Audit;
using LinkPara.ContextProvider;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Application.Common.Models.AuthorizationModels;
using LinkPara.Identity.Application.Features.Screens.Commands.UpdateRoleScreen;
using LinkPara.Identity.Domain.Entities;
using LinkPara.Identity.Domain.Enums;
using LinkPara.SharedModels.Authorization.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Data;
using static MassTransit.ValidationResultExtensions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace LinkPara.Identity.Application.Features.Screens.Commands.CreateRoleScreen
{
    public class CreateRoleScreenCommand : IRequest
    {
        public Guid[] ScreensId { get; set; }
        public string Name { get; set; }
        public bool CanSeeSensitiveData { get; set; }
        public RoleScope RoleScope { get; set; }
    }
    public class CreateRoleScreenCommandHandler : IRequestHandler<CreateRoleScreenCommand>
    {
        private readonly RoleManager<Role> _roleManager;
        private readonly IRepository<Screen> _screenRepository;
        private readonly IRepository<RoleScreen> _roleScreenRepository;
        private readonly IRepository<ScreenClaim> _screenClaim;
        private readonly IClaimService _claimService;
        private readonly IAuditLogService _auditLogService;
        private readonly IContextProvider _contextProvider;
        private readonly IRepository<Permission> _permissionRepository;
        public CreateRoleScreenCommandHandler(RoleManager<Role> roleManager,
            IRepository<Screen> screenRepository,
            IRepository<RoleScreen> roleScreenRepository,
            IRepository<ScreenClaim> screenClaim,
            IClaimService claimService,
            IRepository<Permission> permissionRepository,
            IAuditLogService auditLogService,
            IContextProvider contextProvider)
        {
            _roleManager = roleManager;
            _screenRepository = screenRepository;
            _roleScreenRepository = roleScreenRepository;
            _screenClaim = screenClaim;
            _claimService = claimService;
            _permissionRepository = permissionRepository;
            _auditLogService = auditLogService;
            _contextProvider = contextProvider;
        }

        public async Task<Unit> Handle(CreateRoleScreenCommand command, CancellationToken cancellationToken)
        {
            if (await _roleManager.RoleExistsAsync(command.Name))
            {
                throw new AlreadyInUseException(command.Name);
            }

            var userId = _contextProvider.CurrentContext.UserId;
            var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

            var role = new Role
            {
                Name = command.Name,
                RoleScope = command.RoleScope,
                CanSeeSensitiveData = command.CanSeeSensitiveData,
                RecordStatus = RecordStatus.Active,
                CreateDate = DateTime.Now,
                CreatedBy = parseUserId.ToString()
            };

            var result = await _roleManager.CreateAsync(role);

            await AddScreensToRole(role, command, result, parseUserId);

            await _auditLogService.AuditLogAsync(
               new AuditLog
               {
                   IsSuccess = true,
                   LogDate = DateTime.Now,
                   Operation = "CreateRole",
                   SourceApplication = "Identity",
                   Resource = "Role",
                   UserId = parseUserId,
                   Details = new Dictionary<string, string>
                   {
                                    {"Name", command.Name },
                                    {"RoleScope", command.RoleScope.ToString() },
                   }
               });

            var matchingScreenClaims = await GetMatchRoleScreenClaims(command);

            foreach (var matchingClaim in matchingScreenClaims)
            {
                await _claimService.AddClaimToRoleAsync(role, new List<PermissionOperationType> { matchingClaim.OperationType }, matchingClaim.Module);
            }

            await AddDisplayFalseClaims(role);


            await CheckExistingDashboardScreen(role, parseUserId);

            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = true,
                    LogDate = DateTime.Now,
                    Operation = "AddedClaimsToRole",
                    SourceApplication = "Identity",
                    Resource = "RoleScreen",
                    UserId = parseUserId,
                    Details = new Dictionary<string, string>
                    {
                    {"Name", command.Name }
                    }
                });
            return Unit.Value;
        }
        private async Task CheckExistingDashboardScreen(Role role, Guid userId)
        {
            var dashboardName = role.RoleScope switch
            {
                RoleScope.Internal => "DASHBOARD.TITLE",
                RoleScope.CorporateSubMerchant => "SUBMERCHANT.DASHBOARD",
                _ => "MERCHANT.DASHBOARD"
            };

            var existingDashboardScreen = await _roleScreenRepository.GetAll(rs => rs.Screen)
                .FirstOrDefaultAsync(rs => rs.RoleId == role.Id &&
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
                    RoleId = role.Id,
                    ScreenId = dashboardScreen.Id,
                    CreatedBy = userId.ToString()
                };

                await _roleScreenRepository.AddAsync(roleScreen);
            }
        }
        private async Task<List<Permission>> GetMatchRoleScreenClaims(CreateRoleScreenCommand command)
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
        private async Task AddDisplayFalseClaims(Role role)
        {
            var permissions = await _permissionRepository.GetAll().Where(b => b.Display == false).ToListAsync();
            if (permissions.Any())
            {
                foreach (var permission in permissions)
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

        private async Task AddScreensToRole(Role role, CreateRoleScreenCommand command, IdentityResult result, Guid parseUserId)
        {
            if (result.Succeeded && command.ScreensId.Any())
            {
                foreach (var screenId in command.ScreensId)
                {
                    var roleScreen = new RoleScreen
                    {
                        RoleId = role.Id,
                        ScreenId = screenId,
                        RecordStatus = RecordStatus.Active,
                        CreateDate = DateTime.Now,
                        CreatedBy = parseUserId.ToString()
                    };
                    await _roleScreenRepository.AddAsync(roleScreen);
                }

            }
        }
    }
}

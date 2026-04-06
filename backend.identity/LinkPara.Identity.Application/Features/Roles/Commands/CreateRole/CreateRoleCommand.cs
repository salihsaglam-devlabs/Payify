using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Application.Common.Models.AuthorizationModels;
using LinkPara.Identity.Domain.Entities;
using LinkPara.Identity.Domain.Enums;
using LinkPara.SharedModels.Authorization.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Identity.Application.Features.Roles.Commands.CreateRole;

public class CreateRoleCommand : IRequest
{
    public string Name { get; set; }
    public RoleScope RoleScope { get; set; }
    public bool CanSeeSensitiveData { get; set; }
    public ModuleClaim[] Claims { get; set; }
}

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand>
{
    private readonly RoleManager<Role> _roleManager;
    private readonly IRepository<Role> _roleRepository;
    private readonly IClaimService _claimService;
    private readonly IAuditLogService _auditLogService;
    private readonly IContextProvider _contextProvider;
    private readonly IRepository<Permission> _permissionRepository;

    public CreateRoleCommandHandler(RoleManager<Role> roleManager,
        IClaimService claimService,
        IAuditLogService auditLogService,
        IContextProvider contextProvider,
        IRepository<Permission> permissionRepository,
        IRepository<Role> roleRepository)
    {
        _roleManager = roleManager;
        _claimService = claimService;
        _auditLogService = auditLogService;
        _contextProvider = contextProvider;
        _permissionRepository = permissionRepository;
        _roleRepository = roleRepository;
    }

    public async Task<Unit> Handle(CreateRoleCommand command, CancellationToken cancellationToken)
    {
        var isExist = await _roleRepository.GetAll()
            .AnyAsync(s => s.Name == command.Name && s.RecordStatus == RecordStatus.Active);

        if (isExist)
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

        if (result.Succeeded)
        {

            foreach (var claim in command.Claims)
            {
                await _claimService.AddClaimToRoleAsync(role, claim.PermissionOperationTypes, claim.Module);
            }

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

        }
        else
        {

            await _auditLogService.AuditLogAsync(
                 new AuditLog
                 {
                     IsSuccess = false,
                     LogDate = DateTime.Now,
                     Operation = "CreateRole",
                     SourceApplication = "Identity",
                     Resource = "Role",
                     UserId = parseUserId,
                     Details = new Dictionary<string, string>
                     {
                          {"Name", command.Name },
                          {"Description", command.RoleScope.ToString() },
                          {"ErrorMessage" , result.Errors.FirstOrDefault()?.Description}
                     }
                 });

            throw new InvalidDataException(result.Errors.FirstOrDefault()?.Description);
        }

        return Unit.Value;
    }
}
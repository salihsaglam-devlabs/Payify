using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Application.Features.Users.Queries;
using MediatR;
using LinkPara.SharedModels.Exceptions;
using LinkPara.Identity.Domain.Entities;

namespace LinkPara.Identity.Application.Features.Permissions.Commands.UpdatePermission;

public class UpdatePermissionCommand : IRequest
{
    public Guid Id { get; set; }
    public UpdatePermissionDto UpdatePermission { get; set; }
}

public class UpdatePermissionCommandHandler : IRequestHandler<UpdatePermissionCommand>
{
    private readonly IPermissionService _permissionService;
    private readonly IAuditLogService _auditLogService;
    private readonly IContextProvider _contextProvider;

    public UpdatePermissionCommandHandler(IPermissionService permissionService,
        IAuditLogService auditLogService,
        IContextProvider contextProvider)
    {
        _permissionService = permissionService;
        _auditLogService = auditLogService;
        _contextProvider = contextProvider; 
    }

    public async Task<Unit> Handle(UpdatePermissionCommand command, CancellationToken cancellationToken)
    {
        var permission = await _permissionService.GetPermissionByIdAsync(command.Id);

        if (permission is null)
        {
            throw new NotFoundException(nameof(Permission));
        }

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        if (permission.DisplayName != command.UpdatePermission.DisplayName)
        {
            var ownerModuleList = await _permissionService.GetPermissionOwnerModulesByIdAsync(command.Id);

            foreach (var ownerModule in ownerModuleList)
            {
                ownerModule.DisplayName = command.UpdatePermission.DisplayName;

                var auditChanges = new Dictionary<string, string>();
                auditChanges.Add("DisplayName", command.UpdatePermission.DisplayName);

                if (ownerModule.Id == permission.Id)
                {
                    ownerModule.Description = command.UpdatePermission.Description;
                    ownerModule.Display = command.UpdatePermission.Display;

                    auditChanges.Add("Description", command.UpdatePermission.Description);
                    auditChanges.Add("Display", command.UpdatePermission.Display.ToString());
                }

                await _permissionService.UpdatePermissionAsync(ownerModule);

                await _auditLogService.AuditLogAsync(
                    new AuditLog
                    {
                        IsSuccess = true,
                        LogDate = DateTime.Now,
                        Operation = "UpdatePermission",
                        SourceApplication = "Identity",
                        Resource = "Permission",
                        UserId = parseUserId,
                        Details = auditChanges
                    });
            }
        }

        else
        {
            permission.Display = command.UpdatePermission.Display;
            permission.Description = command.UpdatePermission.Description;

            await _permissionService.UpdatePermissionAsync(permission);

            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = true,
                    LogDate = DateTime.Now,
                    Operation = "UpdatePermission",
                    SourceApplication = "Identity",
                    Resource = "Permission",
                    UserId = parseUserId,
                    Details = new Dictionary<string, string>
                    {
                          {"Description", command.UpdatePermission.Description },
                          {"DisplayName", command.UpdatePermission.DisplayName },
                    }
                });
        }

        return Unit.Value;
    }
}
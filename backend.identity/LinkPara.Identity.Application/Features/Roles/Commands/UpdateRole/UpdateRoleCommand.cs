using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Application.Common.Models.AuthorizationModels;
using LinkPara.Identity.Domain.Entities;
using LinkPara.Identity.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace LinkPara.Identity.Application.Features.Roles.Commands.UpdateRole;

public class UpdateRoleCommand : IRequest
{
    public string RoleId { get; set; }
    public string Name { get; set; }
    public bool CanSeeSensitiveData { get; set; }
    public RoleScope RoleScope { get; set; }

    public ModuleClaim[] Claims { get; set; }
}

public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand>
{
    private readonly RoleManager<Role> _roleManager;
    private readonly IAuditLogService _auditLogService;
    private readonly IContextProvider _contextProvider;

    public UpdateRoleCommandHandler(RoleManager<Role> roleManager,
        IAuditLogService auditLogService,
        IContextProvider contextProvider
        )
    {
        _roleManager = roleManager;
        _auditLogService = auditLogService;
        _contextProvider = contextProvider;
    }

    public async Task<Unit> Handle(UpdateRoleCommand command, CancellationToken cancellationToken)
    {
        var role = await _roleManager.FindByIdAsync(command.RoleId);

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        if (role is null)
        {
            throw new NotFoundException(nameof(Role), command.RoleId);
        }

        role.RecordStatus = RecordStatus.Active;
        role.UpdateDate = DateTime.Now;
        role.LastModifiedBy = parseUserId.ToString();

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
                    UserId = parseUserId,
                    Details = new Dictionary<string, string>
                    {
                            {"Name", command.Name },
                            {"ErrorMessage" , updateResult.Errors.FirstOrDefault().Description}
                    }
                });

            throw new InvalidDataException(updateResult.Errors.FirstOrDefault()?.Description);
        }

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
}
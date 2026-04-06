using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.Identity.Application.Common.Exceptions;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace LinkPara.Identity.Application.Features.Roles.Commands.DeleteRole;

public class DeleteRoleCommand : IRequest
{
    public Guid RoleId { get; set; }
}

public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand>
{
    private readonly UserManager<User> _userManager;
    private readonly IAuditLogService _auditLogService;
    private readonly IContextProvider _contextProvider;
    private readonly IRepository<Role> _roleRepository;

    public DeleteRoleCommandHandler(
        UserManager<User> userManager,
        IAuditLogService auditLogService,
        IContextProvider contextProvider,
        IRepository<Role> roleRepository)
    {
        _userManager = userManager;
        _auditLogService = auditLogService;
        _contextProvider = contextProvider;
        _roleRepository = roleRepository;
    }

    public async Task<Unit> Handle(DeleteRoleCommand command, CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByIdAsync(command.RoleId);

        if (role is null)
        {
            throw new NotFoundException(nameof(Role), command.RoleId.ToString());
        }

        var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
        if (usersInRole.Count > 0)
        {
            throw new RoleHasUsersException();
        }

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        try
        {
            await _roleRepository.DeleteAsync(role);
        }
        catch (Exception exception)
        {
            await _auditLogService.AuditLogAsync(
              new AuditLog
              {
                  IsSuccess = false,
                  LogDate = DateTime.Now,
                  Operation = "DeleteRole",
                  SourceApplication = "Identity",
                  Resource = "Role",
                  UserId = parseUserId,
                  Details = new Dictionary<string, string>
                  {
                     {"RoleId", command.RoleId.ToString() },
                     {"ErrorMessage" , exception.Message}
                  }
              });

            throw new ArgumentException();
        }

        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "DeleteRole",
                SourceApplication = "Identity",
                Resource = "Role",
                UserId = parseUserId,
                Details = new Dictionary<string, string>
                {
                    {"RoleId", command.RoleId.ToString() },
                }
            });

        return Unit.Value;
    }
}
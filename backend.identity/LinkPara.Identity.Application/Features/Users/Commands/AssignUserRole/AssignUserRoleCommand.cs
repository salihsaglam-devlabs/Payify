using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace LinkPara.Identity.Application.Features.Users.Commands.AssignUserRole;

public class AssignUserRoleCommand : IRequest
{
    public Guid UserId { get; set; }
    public List<Guid> Roles { get; set; }
}

public class AssignUserRoleCommandHandler : IRequestHandler<AssignUserRoleCommand>
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IAuditLogService _auditLogService;
    private readonly IContextProvider _contextProvider;


    public AssignUserRoleCommandHandler(
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        IAuditLogService auditLogService,
        IContextProvider contextProvider)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _auditLogService = auditLogService;
        _contextProvider = contextProvider;
    }

    public async Task<Unit> Handle(AssignUserRoleCommand command, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(command.UserId.ToString());

        if (user is null)
        {
            throw new NotFoundException(nameof(User), command.UserId);
        }

        var roleNames = new List<string>();

        var newRoles = await _roleManager.Roles.Where(x => command.Roles.Contains(x.Id)).ToListAsync();

        foreach (var role in newRoles)
        {
            if (role is null)
            {
                throw new NotFoundException(nameof(Role), role);
            }

            roleNames.Add(role.Name);
        }

        var userRoles = await _userManager.GetRolesAsync(user);

        var removedOldRoles = await _userManager.RemoveFromRolesAsync(user, userRoles);

        var result = await _userManager.AddToRolesAsync(user, roleNames);

        if (!result.Succeeded)
        {
            await _userManager.AddToRolesAsync(user, userRoles);

            throw new InvalidOperationException(result.Errors.FirstOrDefault()?.Code);
        }

        var userId = !string.IsNullOrWhiteSpace(_contextProvider.CurrentContext.UserId) ? Guid.Parse(_contextProvider.CurrentContext.UserId) : Guid.Empty;

        await _auditLogService.AuditLogAsync(
        new AuditLog
        {
            IsSuccess = true,
            LogDate = DateTime.Now,
            Operation = "AssignUserRole",
            SourceApplication = "Identity",
            Resource = "UserRole",
            UserId = userId,
            Details = new Dictionary<string, string>
            {
                 {"Roles", string.Join("," , command.Roles) },
                 {"UserId", command.UserId.ToString() }
            }
        });

        return Unit.Value;
    }
}
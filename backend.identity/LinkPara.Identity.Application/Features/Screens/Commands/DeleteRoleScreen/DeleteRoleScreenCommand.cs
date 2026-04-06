using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.Identity.Application.Common.Exceptions;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace LinkPara.Identity.Application.Features.Screens.Commands.DeleteRoleScreen
{
    public class DeleteRoleScreenCommand : IRequest
    {
        public Guid RoleId { get; set; }
    }
    public class DeleteRoleScreenCommandHandler : IRequestHandler<DeleteRoleScreenCommand>
    {
        private readonly UserManager<User> _userManager;
        private readonly IAuditLogService _auditLogService;
        private readonly IContextProvider _contextProvider;
        private readonly IScreenService _screenService;
        private readonly IRepository<Role> _roleRepository;


        public DeleteRoleScreenCommandHandler(
            UserManager<User> userManager,
            IAuditLogService auditLogService,
            IScreenService screenService,
            IContextProvider contextProvider,
            IRepository<Role> roleRepository)
        {
            _userManager = userManager;
            _auditLogService = auditLogService;
            _screenService = screenService;
            _contextProvider = contextProvider;
            _roleRepository = roleRepository;
        }

        public async Task<Unit> Handle(DeleteRoleScreenCommand command, CancellationToken cancellationToken)
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

            await _screenService.DeleteRoleScreensByRoleIdAsync(command.RoleId);

            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = true,
                    LogDate = DateTime.Now,
                    Operation = "DeleteRoleScreen",
                    SourceApplication = "Identity",
                    Resource = "RoleScreen",
                    UserId = parseUserId,
                    Details = new Dictionary<string, string>
                    {
                    {"RoleId", command.RoleId.ToString() },
                    }
                });

            return Unit.Value;
        }
    }
}
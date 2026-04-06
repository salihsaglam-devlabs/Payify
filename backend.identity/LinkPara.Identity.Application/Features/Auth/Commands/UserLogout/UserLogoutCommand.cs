using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Identity.Application.Features.Auth.Commands.UserLogout
{
    public class UserLogoutCommand : IRequest
    {
        public Guid UserId { get; set; }
        public string RefreshToken { get; set; }
    }
    public class UserLogoutCommandHandler : IRequestHandler<UserLogoutCommand>
    {
        private readonly IRepository<UserSession> _userSessionTokenRepository;
        private readonly IAuditLogService _auditLogService;
        private readonly UserManager<User> _userManager;
        public UserLogoutCommandHandler(IRepository<UserSession> userSessionTokenRepository,
            IAuditLogService auditLogService,
            UserManager<User> userManager)
        {
            _userSessionTokenRepository = userSessionTokenRepository;
            _auditLogService = auditLogService;
            _userManager = userManager;
        }

        public async Task<Unit> Handle(UserLogoutCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());

            if (user is null)
            {
                throw new NotFoundException(nameof(User), request.UserId);
            }
            
            var existRefreshToken = await _userSessionTokenRepository.GetAll()
                .Where(q => q.UserId == request.UserId && q.RefreshToken == request.RefreshToken)
                .FirstOrDefaultAsync();

            if (existRefreshToken is not null)
            {
                await _userSessionTokenRepository.DeleteAsync(existRefreshToken);
            }
            
            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = true,
                    LogDate = DateTime.Now,
                    Operation = "UserLogoutSucceeded ",
                    SourceApplication = "Identity",
                    Resource = "User",
                    UserId = user.Id,
                    Details = new Dictionary<string, string>
                    {
                        {"UserId", user.Id.ToString()},
                        {"UserName", user.UserName },
                        {"Email", user.Email },
                    }
                });

            return Unit.Value;
        }
    }
}

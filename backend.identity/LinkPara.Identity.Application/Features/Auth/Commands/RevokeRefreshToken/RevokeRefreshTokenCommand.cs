using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Identity.Application.Features.Auth.Commands.RevokeRefreshToken
{
    public class RevokeRefreshTokenCommand : IRequest
    {
        public Guid UserId { get; set; }
    }
    public class RevokeRefreshTokenCommandHandler : IRequestHandler<RevokeRefreshTokenCommand>
    {
        private readonly IRepository<UserSession> _userSessionTokenRepository;
        private readonly IAuthService _authService;
        public RevokeRefreshTokenCommandHandler(IRepository<UserSession> userSessionTokenRepository, 
            IAuthService authService)
        {
            _userSessionTokenRepository = userSessionTokenRepository;
            _authService = authService;
        }

        public async Task<Unit> Handle(RevokeRefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var existRefreshToken = await _userSessionTokenRepository.GetAll()
               .Where(q => q.UserId == request.UserId)
                  .FirstOrDefaultAsync();

            if (existRefreshToken == null)
            {
                throw new NotFoundException("User Refresh Token Not Found");
            }

            await _authService.DeleteRefreshToken(existRefreshToken);

            return Unit.Value;
        }
    }
}

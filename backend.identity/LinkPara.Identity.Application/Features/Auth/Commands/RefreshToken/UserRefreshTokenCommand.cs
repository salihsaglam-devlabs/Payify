using LinkPara.HttpProviders.Vault;
using LinkPara.Identity.Application.Common.Exceptions;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Application.Common.Models.IdentityConfiguration;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Identity.Application.Features.OAuth.Commands.RefreshToken
{
    public class UserRefreshTokenCommand : IRequest<UserTokenDto>
    {
        public Guid UserId { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiration { get; set; }
    }
    public class LoginUserCommandHandler : IRequestHandler<UserRefreshTokenCommand, UserTokenDto>
    {
        private readonly IRepository<UserSession> _userSessionTokenRepository;
        private readonly IVaultClient vaultClient;
        private readonly IJwtHelper _jwtHelper;
        private readonly UserManager<User> _userManager;
        private readonly IAuthService _authService;
        public LoginUserCommandHandler(IRepository<UserSession> userSessionTokenRepository,
            IAuthService authService,
            IJwtHelper jwtHelper,
            UserManager<User> userManager,
            IVaultClient vaultClient)
        {
            _userSessionTokenRepository = userSessionTokenRepository;
            _authService = authService;
            _jwtHelper = jwtHelper;
            _userManager = userManager;
            this.vaultClient = vaultClient;
        }

        public async Task<UserTokenDto> Handle(UserRefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString()) ?? throw new UserNotFoundException();

            var userSession = await _userSessionTokenRepository.GetAll()
                        .Where(u => u.UserId == request.UserId
                        && u.RefreshToken == request.RefreshToken)
                        .FirstOrDefaultAsync(cancellationToken: cancellationToken) ?? throw new AuthorizationException();

            if (userSession.RefreshTokenExpiration > DateTime.UtcNow && userSession.RecordStatus == RecordStatus.Passive)
            {
                await _authService.DeleteRefreshToken(userSession);

                throw new NewSessionOpenedException();
            }

            if (userSession.RefreshTokenExpiration < DateTime.UtcNow)
            {
                await _userSessionTokenRepository.DeleteAsync(userSession);

                throw new SessionExpiredException();
            }

            var accessToken = await _jwtHelper.GenerateJwtTokenAsync(user, false, userSession.Id.ToString(), userSession);

            var tokenSettings = vaultClient.GetSecretValue<TokenSettings>("SharedSecrets", "JwtConfiguration", "TokenSettings");
            userSession.RefreshTokenExpiration = DateTime.SpecifyKind(    userSession.RefreshTokenExpiration.AddMinutes(tokenSettings.TokenExpiryDefaultMinute),DateTimeKind.Utc);

            await _userSessionTokenRepository.UpdateAsync(userSession);

            return new UserTokenDto
            {
                UserId = userSession.UserId,
                AccessToken = accessToken,
                RefreshTokenExpiration = userSession.RefreshTokenExpiration,
                RefreshToken = userSession.RefreshToken,
            };
        }
    }
}
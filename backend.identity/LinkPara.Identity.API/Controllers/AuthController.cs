using LinkPara.Identity.Application.Features.Auth;
using LinkPara.Identity.Application.Features.Auth.Commands.CheckPassword;
using LinkPara.Identity.Application.Features.Auth.Commands.GenerateToken;
using LinkPara.Identity.Application.Features.Auth.Commands.MultiFactorDeviceActivation;
using LinkPara.Identity.Application.Features.Auth.Commands.RevokeRefreshToken;
using LinkPara.Identity.Application.Features.Auth.Commands.UserLogin;
using LinkPara.Identity.Application.Features.Auth.Commands.UserLogout;
using LinkPara.Identity.Application.Features.Auth.Queries.GetUserSessionById;
using LinkPara.Identity.Application.Features.OAuth;
using LinkPara.Identity.Application.Features.OAuth.Commands.RefreshToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Identity.API.Controllers
{
    public class AuthController : ApiControllerBase
    {
        /// <summary>
        /// Generate an access token with externalcustomerid & personid 
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("generate-token")]
        public async Task<ActionResult<UserTokenDto>> GenerateTokenAsync(GenerateTokenCommand command)
        {
            return await Mediator.Send(command);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<UserTokenDto>> LoginAsync(UserLoginCommand command)
        {
            return await Mediator.Send(command);
        }

        [Authorize]
        [HttpPost("check-password")]
        public async Task<ActionResult<CheckPasswordDto>> CheckPasswordAsync(CheckPasswordCommand command)
        {
            return await Mediator.Send(command);
        }

        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<ActionResult<UserTokenDto>> RefreshToken(UserRefreshTokenCommand command)
        {
            return await Mediator.Send(command);
        }

        [AllowAnonymous]
        [HttpPost("logout")]
        public async Task<ActionResult> Logout(UserLogoutCommand command)
        {
            await Mediator.Send(command);
            return NoContent();
        }

        [Authorize(Policy = "UserRefreshToken:Delete")]
        [HttpPost("revoke-refresh-token")]
        public async Task<ActionResult> RevokeRefreshToken(RevokeRefreshTokenCommand command)
        {
            await Mediator.Send(command);
            return NoContent();
        }

        [AllowAnonymous]
        [HttpGet("session/{sessionId}")]
        public async Task<ActionResult<UserSessionDto>> GetUserSessionAsync(Guid sessionId)
        {
            return await Mediator.Send(new GetUserSessionByIdQuery { SessionId = sessionId });
        }

        [AllowAnonymous]
        [HttpPost("multifactor-activation")]
        public async Task MultifactorActivationAsync(MultifactorAuthCommand command)
        {
            await Mediator.Send(command);
        }
    }
}

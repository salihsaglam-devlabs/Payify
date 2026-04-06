using LinkPara.ApiGateway.BackOffice.Commons.Models.AuthorizationModels;
using LinkPara.ApiGateway.BackOffice.Filters.LoginActionFilter;
using LinkPara.ApiGateway.BackOffice.Services.Identity.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Responses;
using LinkPara.ApiGateway.BackOffice.Services.Notification.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Enums;
using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Responses;
using LinkPara.ApiGateway.Commons.Helpers;
using LinkPara.ContextProvider;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Identity
{
    public class AuthController : ApiControllerBase
    {
        private readonly IOtpHttpClient _otpHttpClient;
        private readonly IUserNameGenerator _userNameGenerator;
        private readonly IAuthHttpClient _oAuthHttpClient;
        private readonly IContextProvider _contextProvider;
        private readonly IConfiguration _configuration;
        
        public AuthController(
        IOtpHttpClient otpHttpClient,
        IUserNameGenerator userNameGenerator,
        IAuthHttpClient oAuthHttpClient,
         IContextProvider contextProvider, 
        IConfiguration configuration)
        {
            _otpHttpClient = otpHttpClient;
            _userNameGenerator = userNameGenerator;
            _oAuthHttpClient = oAuthHttpClient;
            _contextProvider = contextProvider;
            _configuration = configuration;
        }
        
        [HttpPost("login")]
        [ServiceFilter(typeof(LoginActionFilter))]
        public async Task<ActionResult<TokenDto>> LoginAsync(LoginOtpRequest request)
        {
            bool isOtpVerificationNeeded = await CheckVerificationNeededAsync(request);

            if (isOtpVerificationNeeded)
            {
                var result = await _otpHttpClient.VerifyOtpAsync(new VerifyOtpRequest
                {
                    Code = request.Code,
                    TimeStamp = request.TimeStamp,
                    OtpAuthorizationId = request.OtpAuthorizationId
                });

                if (!result.Success)
                {
                    throw new InvalidOtpException();
                }
            }
            
            request.Username = await _userNameGenerator.GetUserName(request.Username, _contextProvider.CurrentContext.Channel);

            return await _oAuthHttpClient.LoginAsync(new LoginRequest
            {
                Username = request.Username,
                Password = request.Password
            });
        }

        [HttpPost("verify-otp")]
        public async Task<ActionResult<VerifyOtpResponse>> VerifyOtpAsync(LoginOtpRequest request)
        {
            bool isOtpVerificationNeeded = await CheckVerificationNeededAsync(request);

            var otpResponse = new VerifyOtpResponse(true,"");

            if (isOtpVerificationNeeded)
            {
                otpResponse = await _otpHttpClient.VerifyOtpAsync(new VerifyOtpRequest
                {
                    Code = request.Code,
                    TimeStamp = request.TimeStamp,
                    OtpAuthorizationId = request.OtpAuthorizationId
                });

                if (!otpResponse.Success)
                {
                    throw new InvalidOtpException();
                }
            }

            return otpResponse;
        }

        private async Task<bool> CheckVerificationNeededAsync(LoginOtpRequest request)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            
            if (environment?.ToLowerInvariant() == "development")
            {
                return false;
            }
            
            if (environment.ToLowerInvariant() == "staging")
            {
                if (await CheckDefaultUser(request))
                {
                    return false;
                }
            }

            return true;
        }
        
        private async Task<bool> CheckDefaultUser(LoginOtpRequest request)
        {
            var defaultUsers = _configuration.GetSection("DefaultUsers").Get<List<DefaultUser>>();

            var matchingUser = defaultUsers.FirstOrDefault(user => user.PhoneNumber == request.Username 
                                                                   && user.Otp == request.Code.ToString());
            return await Task.FromResult(matchingUser is not null);
        }

        [HttpPost("login-otp")]
        [ServiceFilter(typeof(LoginActionFilter))]
        public async Task<SendOtpResponse> SendLoginOtpAsync(LoginRequest request)
        {
            var phoneNumber = request.Username;

            request.Username = await _userNameGenerator.GetUserName(request.Username, _contextProvider.CurrentContext.Channel);

            var loginResult = await _oAuthHttpClient.LoginAsync(request);

            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (environment?.ToLowerInvariant() == "development")
            {
                return new SendOtpResponse
                {
                    ExpirationDate = DateTime.Now.AddMinutes(3),
                    TimeStamp = Guid.NewGuid().ToString(),
                    OtpAuthorizationId = Guid.NewGuid().ToString(),
                    IsSuccess = true
                };
            }

            var otpRequest = new SendOtpRequest()
            {
                Action = OtpAction.Login,
                OtpType = OtpType.Sms,
                Receiver = phoneNumber,
                Username = request.Username
            };

            return await _otpHttpClient.SendOtpAsync(otpRequest);
        }
        
        [AllowAnonymous]
        [HttpPost("logout")]
        public async Task LogoutAsync(LogoutRequest request)
        {
            await _oAuthHttpClient.LogoutAsync(request);
        }

        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<ActionResult<UserRefreshTokenResponse>> RefreshToken(UserRefreshTokenRequest command)
        {
            return await _oAuthHttpClient.RefreshTokenAsync(command);
        }

        [Authorize(Policy = "UserRefreshToken:Delete")]
        [HttpPost("revoke-refresh-token")]
        public async Task RevokeRefreshToken(RevokeRefreshTokenRequest request)
        {
            await _oAuthHttpClient.RevokeRefreshTokenAsync(request);
        }

        [Authorize(Policy = "UserRefreshToken:ReadAll")]
        [HttpGet("")]
        public async Task<ActionResult<PaginatedList<ActiveUserDto>>> GetAllActiveUsersAsync([FromQuery] AllActiveUserRequest query)
        {
            return await _oAuthHttpClient.GetAllActiveUsersAsync(query);        
        }
    }
}

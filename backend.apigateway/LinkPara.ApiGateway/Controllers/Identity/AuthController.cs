using AutoMapper;
using LinkPara.ApiGateway.Commons.Helpers;
using LinkPara.ApiGateway.Services.Emoney.HttpClients;
using LinkPara.ApiGateway.Services.Identity.HttpClients;
using LinkPara.ApiGateway.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.Services.Identity.Models.Responses;
using LinkPara.ApiGateway.Services.Notification.HttpClients;
using LinkPara.ApiGateway.Services.Notification.Models.Enums;
using LinkPara.ApiGateway.Services.Notification.Models.Requests;
using LinkPara.ApiGateway.Services.Notification.Models.Responses;
using LinkPara.HttpProviders.Identity;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace LinkPara.ApiGateway.Controllers.Identity
{
    public class AuthController : ApiControllerBase
    {
        private readonly IOtpHttpClient _otpHttpClient;
        private readonly IUserNameGenerator _userNameGenerator;
        private readonly IAuthHttpClient _authHttpClient;
        private readonly IEmoneyAccountHttpClient _emoneyAccountHttpClient;
        private readonly IBus _bus;
        private readonly ILogger<AuthController> _logger;
        private readonly IMapper _mapper;
        private readonly IRecaptchaService _recaptchaService;
        public AuthController(
        IOtpHttpClient otpHttpClient,
        IUserNameGenerator userNameGenerator,
        IAuthHttpClient authhttpClient,
        IEmoneyAccountHttpClient emoneyAccountHttpClient,
        IBus bus,
        ILogger<AuthController> logger,
        IMapper mapper,
        IRecaptchaService recaptchaService)
        {
            _otpHttpClient = otpHttpClient;
            _userNameGenerator = userNameGenerator;
            _authHttpClient = authhttpClient;
            _emoneyAccountHttpClient = emoneyAccountHttpClient;
            _bus = bus;
            _logger = logger;
            _mapper = mapper;
            _recaptchaService = recaptchaService;
        }

        [Authorize(Policy = "RequireOtp")]
        [HttpPost("login")]
        public async Task<ActionResult<TokenDto>> LoginAsync(LoginRequest request)
        {
            request.Username = await _userNameGenerator.GetUserNameAsync(request.Username);

            return await _authHttpClient.LoginAsync(request);
        }

        /// <summary>
        /// check the password.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("check-password")]
        public async Task<ActionResult<CheckPasswordResponse>> CheckPasswordAsync(CheckPasswordRequest request)
        {
            return await _authHttpClient.CheckPasswordAsync(request);
        }

        [HttpPost]
        [Route("~/v2/Auth/login")]
        public async Task<ActionResult<TokenDto>> LoginV2Async(LoginRequest request)
        {
            request.Username = await _userNameGenerator.GetUserNameAsync(request.Username);

            return await _authHttpClient.LoginAsync(request);
        }

        /// <summary>
        /// Logout user.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("logout")]
        public async Task LogoutAsync(LogoutRequest request)
        {
            await _authHttpClient.LogoutAsync(request);
        }

        /// <summary>
        /// Get a new accestoken.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<ActionResult<UserRefreshTokenResponse>> RefreshToken(UserRefreshTokenRequest command)
        {
            return await _authHttpClient.RefreshTokenAsync(command);
        }

        [AllowAnonymous]
        [HttpPost("login-otp")]
        public async Task<SendOtpResponse> SendLoginOtpAsync(LoginRequest request)
        {
            await _recaptchaService.VerifyAsync(request.RecaptchaToken);

            var phone = request.Username;

            request.Username = await _userNameGenerator.GetUserNameAsync(request.Username);

            var loginResult = await _authHttpClient.LoginAsync(request);

            var otpRequest = new SendOtpRequest()
            {
                Action = OtpAction.Login,
                OtpType = OtpType.Sms,
                Receiver = phone,
                Username = request.Username
            };

            return await _otpHttpClient.SendOtpAsync(otpRequest);
        }

        [AllowAnonymous]
        [HttpPost("remember-me")]
        public async Task<ActionResult<TokenDto>> RefreshTokenAsync(RefreshTokenRequest request)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(request.LastToken);

            var rememberMe = bool.Parse(jwt.Claims
                .First(claim => claim.Type == "RememberMe").Value);

            request.Username = await _userNameGenerator.GetUserNameAsync(request.Username);

            return await _authHttpClient.LoginAsync(new()
            {
                Password = request.Password,
                Username = request.Username,
                RememberMe = true,
                LoginOtp = request.LoginOtp
            });
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("multifactor-activation")]
        public async Task SendLoginOtpV2Async(MultifactorActivationRequest request)
        {
            request.Username = await _userNameGenerator.GetUserNameAsync(request.Username);

            await _authHttpClient.MultifactorActivationAsync(request);
        }
    }
}

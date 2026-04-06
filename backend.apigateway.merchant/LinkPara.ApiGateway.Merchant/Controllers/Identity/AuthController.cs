using LinkPara.ApiGateway.Merchant.Commons.Helpers;
using LinkPara.ApiGateway.Merchant.Services.Identity.HttpClients;
using LinkPara.ApiGateway.Merchant.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Identity.Models.Responses;
using LinkPara.ApiGateway.Merchant.Services.Notification.HttpClients;
using LinkPara.ApiGateway.Merchant.Services.Notification.Models.Enums;
using LinkPara.ApiGateway.Merchant.Services.Notification.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Notification.Models.Responses;
using LinkPara.ContextProvider;
using LinkPara.HttpProviders.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Merchant.Controllers.Identity;

public class AuthController : ApiControllerBase
{
    private readonly IOtpHttpClient _otpHttpClient;
    private readonly IUserNameGenerator _userNameGenerator;
    private readonly IAuthHttpClient _oAuthHttpClient;
    private readonly IContextProvider _contextProvider;
    private readonly IRecaptchaService _recaptchaService;

    public AuthController(
        IOtpHttpClient otpHttpClient,
        IUserNameGenerator userNameGenerator,
        IAuthHttpClient oAuthhttpClient,
        IContextProvider contextProvider,
        IRecaptchaService recaptchaService)
    {
        _otpHttpClient = otpHttpClient;
        _userNameGenerator = userNameGenerator;
        _oAuthHttpClient = oAuthhttpClient;
        _contextProvider = contextProvider;
        _recaptchaService = recaptchaService;
    }


    [HttpPost("login")]
    [Authorize(Policy = "RequireOtp")]
    public async Task<ActionResult<TokenDto>> LoginAsync(LoginRequest request)
    {
        request.Username = await _userNameGenerator.GetUserName(request.Username, _contextProvider.CurrentContext.Channel);

        return await _oAuthHttpClient.LoginAsync(request);
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

    [HttpPost("login-otp")]
    public async Task<SendOtpResponse> SendLoginOtpAsync(LoginRequest request)
    {
        await _recaptchaService.VerifyAsync(request.RecaptchaToken);

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
                IsSuccess = true,
                UserSecurityPictureEnabled = loginResult.UserSecurityPictureEnabled,
                UserSecurityPicture =loginResult.UserSecurityPicture
            };
        }

        var otpRequest = new SendOtpRequest
        {
            Action = OtpAction.Login,
            OtpType = OtpType.Sms,
            Receiver = phoneNumber,
            Username = request.Username
        };

        var otpResponse = await _otpHttpClient.SendOtpAsync(otpRequest);
        otpResponse.UserSecurityPictureEnabled = loginResult.UserSecurityPictureEnabled;
        otpResponse.UserSecurityPicture = loginResult.UserSecurityPicture;
        return otpResponse;
    }

}

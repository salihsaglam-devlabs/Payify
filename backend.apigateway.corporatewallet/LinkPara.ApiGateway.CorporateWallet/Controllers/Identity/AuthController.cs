using LinkPara.ApiGateway.CorporateWallet.Commons.Helpers;
using LinkPara.ApiGateway.CorporateWallet.Services.Identity.HttpClients;
using LinkPara.ApiGateway.CorporateWallet.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.Identity.Models.Responses;
using LinkPara.ApiGateway.CorporateWallet.Services.Notification.HttpClients;
using LinkPara.ApiGateway.CorporateWallet.Services.Notification.Models.Enums;
using LinkPara.ApiGateway.CorporateWallet.Services.Notification.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.Notification.Models.Responses;
using LinkPara.HttpProviders.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.CorporateWallet.Controllers.Identity;

public class AuthController : ApiControllerBase
{
    private readonly IOtpHttpClient _otpHttpClient;
    private readonly IAuthHttpClient _oAuthHttpClient;
    private readonly IUserNameGenerator _userNameGenerator;
    private readonly IRecaptchaService _recaptchaService;

    public AuthController(
        IOtpHttpClient otpHttpClient,
        IAuthHttpClient oAuthhttpClient,
        IUserNameGenerator userNameGenerator,
        IRecaptchaService recaptchaService)
    {
        _otpHttpClient = otpHttpClient;
        _oAuthHttpClient = oAuthhttpClient;
        _userNameGenerator = userNameGenerator;
        _recaptchaService = recaptchaService;
    }


    [Authorize(Policy = "RequireOtp")]
    [HttpPost("login")]
    public async Task<ActionResult<TokenDto>> LoginAsync(LoginRequest request)
    {
        request.Username = await _userNameGenerator.GetUserNameAsync(request.Username);

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

    [AllowAnonymous]
    [HttpPost("login-otp")]
    public async Task<SendOtpResponse> SendLoginOtpAsync(LoginRequest request)
    {
        await _recaptchaService.VerifyAsync(request.RecaptchaToken);

        var phone = request.Username;

        request.Username = await _userNameGenerator.GetUserNameAsync(request.Username);

        var loginResult = await _oAuthHttpClient.LoginAsync(request);

        var otpRequest = new SendOtpRequest()
        {
            Action = OtpAction.Login,
            OtpType = OtpType.Sms,
            Receiver = phone
        };

        return await _otpHttpClient.SendOtpAsync(otpRequest);
    }
}

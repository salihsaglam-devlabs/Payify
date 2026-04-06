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
using LinkPara.SharedModels.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AuthorizeAttribute = Microsoft.AspNetCore.Authorization.AuthorizeAttribute;

namespace LinkPara.ApiGateway.Merchant.Controllers.Identity;

public class AccountController : ApiControllerBase
{
    private readonly IAccountHttpClient _accountHttpClient;
    private readonly IUserHttpClient _userHttpClient;
    private readonly IOtpHttpClient _otpHttpClient;
    private readonly IUserNameGenerator _userNameGenerator;
    private readonly IContextProvider _contextProvider;
    private readonly IRecaptchaService _recaptchaService;

    public AccountController(IAccountHttpClient accountHttpClient,
        IUserHttpClient userHttpClient,
        IOtpHttpClient otpHttpClient,
        IUserNameGenerator userNameGenerator,
        IContextProvider contextProvider,
        IRecaptchaService recaptchaService)
    {
        _accountHttpClient = accountHttpClient;
        _userHttpClient = userHttpClient;
        _otpHttpClient = otpHttpClient;
        _userNameGenerator = userNameGenerator;
        _contextProvider = contextProvider;
        _recaptchaService = recaptchaService;
    }

    /// <summary>
    /// Reset password operation for logged in users.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "User:Update")]
    [HttpPost("reset-password")]
    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        request.UserId = UserId;
        await _accountHttpClient.ResetPasswordAsync(request);
    }

    /// <summary>
    /// Reset password operation for expired passwords. Throws ForbiddenAccessException otherwise.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    /// <exception cref="NotFoundException"></exception>
    /// <exception cref="ForbiddenAccessException"></exception>
    [Authorize(Policy = "RequireOtp")]
    [HttpPost("reset-expired-password")]
    public async Task ResetExpiredPasswordAsync(ResetPasswordRequest request)
    {
        request.UserName = await _userNameGenerator.GetUserName(request.UserName, _contextProvider.CurrentContext.Channel);

        var resetPasswordRequest = new ResetPasswordRequest
        {
            UserName= request.UserName,
            NewPassword = request.NewPassword,
            OldPassword = request.OldPassword,
        };

        await _accountHttpClient.ResetPasswordAsync(resetPasswordRequest);
    }
    
    [AllowAnonymous]
    [HttpPost("reset-password-otp")]
    public async Task<SendOtpResponse> SendResetPasswordOtpAsync(ResetPasswordRequest request)
    {
        var phone = request.UserName;
        
        request.UserName = await _userNameGenerator.GetUserName(request.UserName, _contextProvider.CurrentContext.Channel);
        var users = await _userHttpClient.GetExistingUserListAsync(new GetExistingUsersRequest{UserName = request.UserName});
        if (users.IsExists)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (environment?.ToLowerInvariant() == "development")
            {
                return new SendOtpResponse
                {
                    ExpirationDate = DateTime.Now.AddMinutes(3),
                    TimeStamp = Guid.NewGuid().ToString(),
                    OtpAuthorizationId = Guid.NewGuid().ToString()
                };
            }
            
            var otpRequest = new SendOtpRequest()
            {
                Action = OtpAction.ResetPassword,
                OtpType = OtpType.Sms,
                Receiver = phone,
                Username = request.UserName
            };

            return await _otpHttpClient.SendOtpAsync(otpRequest);
        }
        
        throw new InvalidOperationException();
    }

    /// <summary>
    /// STEP 1: Sends reset password email.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("forgot-password/email")]
    public async Task SendForgotPasswordEmailAsync(SendForgotPasswordEmailRequest request)
    {
        await _recaptchaService.VerifyAsync(request.RecaptchaToken);

        request.UserName = await _userNameGenerator.GetUserName(request.UserName, _contextProvider.CurrentContext.Channel);

        await _accountHttpClient.GetPasswordResetTokenAsync(request);
    }

    /// <summary>
    /// STEP 2: Resets user password after identity verified.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("forgot-password")]
    public async Task ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        await _accountHttpClient.ForgotPasswordAsync(request);
    }
}
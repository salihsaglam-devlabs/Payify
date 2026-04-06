using LinkPara.ApiGateway.CorporateWallet.Commons.Helpers;
using LinkPara.ApiGateway.CorporateWallet.Services.Identity.HttpClients;
using LinkPara.ApiGateway.CorporateWallet.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.Notification.HttpClients;
using LinkPara.ApiGateway.CorporateWallet.Services.Notification.Models.Enums;
using LinkPara.ApiGateway.CorporateWallet.Services.Notification.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.Notification.Models.Responses;
using LinkPara.SharedModels.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AuthorizeAttribute = Microsoft.AspNetCore.Authorization.AuthorizeAttribute;

namespace LinkPara.ApiGateway.CorporateWallet.Controllers.Identity;

public class AccountController : ApiControllerBase
{
    private readonly IAccountHttpClient _accountHttpClient;
    private readonly IUserHttpClient _userHttpClient;
    private readonly IOtpHttpClient _otpHttpClient;
    private readonly IUserNameGenerator _userNameGenerator;

    public AccountController(IAccountHttpClient accountHttpClient,
        IUserHttpClient userHttpClient,
        IOtpHttpClient otpHttpClient,
        IUserNameGenerator userNameGenerator)
    {
        _accountHttpClient = accountHttpClient;
        _userHttpClient = userHttpClient;
        _otpHttpClient = otpHttpClient;
        _userNameGenerator = userNameGenerator;
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
        request.UserName = await _userNameGenerator.GetUserNameAsync(request.UserName);
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
        request.UserName = await _userNameGenerator.GetUserNameAsync(request.UserName);
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
                Receiver = phone
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

        request.UserName = await _userNameGenerator.GetUserNameAsync(request.UserName);
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

    /// <summary>
    /// Checks if the date of birth is valid.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("check-birthdate")]
    public async Task<bool> CheckBirthdateAllowedRange([FromQuery] CheckBirthDateRequest request)
    {
        return await _accountHttpClient.CheckBirthdateAllowedRange(request);
    }
}
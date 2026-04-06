using LinkPara.ApiGateway.BackOffice.Commons.Models.AuthorizationModels;
using LinkPara.ApiGateway.BackOffice.Filters.LoginActionFilter;
using LinkPara.ApiGateway.BackOffice.Services.Identity.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Responses;
using LinkPara.ApiGateway.BackOffice.Services.Notification.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Enums;
using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Responses;
using LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;
using LinkPara.ApiGateway.Commons.Helpers;
using LinkPara.ContextProvider;
using LinkPara.SharedModels.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using AuthorizeAttribute = Microsoft.AspNetCore.Authorization.AuthorizeAttribute;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Identity;

public class AccountController : ApiControllerBase
{
    private readonly IAccountHttpClient _accountHttpClient;
    private readonly IUserHttpClient _userHttpClient;
    private readonly IMerchantHttpClient _merchantHttpClient;
    private readonly IOtpHttpClient _otpHttpClient;
    private readonly IUserNameGenerator _userNameGenerator;
    private readonly IContextProvider _contextProvider;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AccountController(IAccountHttpClient accountHttpClient,
        IUserHttpClient userHttpClient,
        IOtpHttpClient otpHttpClient,
        IUserNameGenerator userNameGenerator,
        IContextProvider contextProvider,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor,
        IMerchantHttpClient merchantHttpClient)
    {
        _accountHttpClient = accountHttpClient;
        _userHttpClient = userHttpClient;
        _otpHttpClient = otpHttpClient;
        _userNameGenerator = userNameGenerator;
        _contextProvider = contextProvider;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _merchantHttpClient = merchantHttpClient;
    }

    /// <summary>
    /// Logins the registered user.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("login")]
    [ServiceFilter(typeof(LoginActionFilter))]
    [Authorize(Policy = "RequireOtp")]    
    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        request.Username = await _userNameGenerator.GetUserName(request.Username, _contextProvider.CurrentContext.Channel);

        return await _accountHttpClient.LoginAsync(request);
    }

    [AllowAnonymous]
    [HttpPost("login-otp")]
    [ServiceFilter(typeof(LoginActionFilter))]
    public async Task<SendOtpResponse> SendLoginOtpAsync(LoginRequest request)
    {
        var phoneNumber = request.Username;

        request.Username = await _userNameGenerator.GetUserName(request.Username, _contextProvider.CurrentContext.Channel);

        var loginResult = await _accountHttpClient.LoginAsync(request);

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
            Action = OtpAction.Login,
            OtpType = OtpType.Sms,
            Receiver = phoneNumber,
            Username = request.Username
        };

        return await _otpHttpClient.SendOtpAsync(otpRequest);
    }

    [AllowAnonymous]
    [HttpPost("send-otp/{merchantId}")]
    public async Task<SendOtpResponse> SendOtpAsync([FromRoute] Guid merchantId)
    {
        var phoneNumber = await _merchantHttpClient.GetAuthorizedPersonPhoneNumberAsync(merchantId);

        var username = await _userNameGenerator.GetUserName(phoneNumber, _contextProvider.CurrentContext.Channel);

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
            Action = OtpAction.Other,
            OtpType = OtpType.Sms,
            Receiver = phoneNumber,
            Username = username
        };

        return await _otpHttpClient.SendOtpAsync(otpRequest);
    }

    /// <summary>
    /// Reset password operation for logged in users.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("reset-password")]
    [Authorize]
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
    [HttpPost("reset-expired-password")]
    public async Task ResetExpiredPasswordAsync(ResetPasswordOtpRequest request)
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
        
        request.UserName = await _userNameGenerator.GetUserName(request.UserName, _contextProvider.CurrentContext.Channel);

        var resetPasswordRequest = new ResetPasswordRequest
        {
            UserName = request.UserName,
            NewPassword = request.NewPassword,
            OldPassword = request.OldPassword
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
    [AllowAnonymous]
    [HttpPost("forgot-password/email")]
    public async Task SendForgotPasswordEmailAsync(SendForgotPasswordEmailRequest request)
    {
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

    /// <summary>
    /// STEP-1: Returns an email update token.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("update-email/token")]
    [Authorize]
    public async Task<GetEmailUpdateTokenResponse> GetEmailUpdateTokenAsync([FromQuery] GetEmailUpdateTokenRequest request)
    {
        return await _accountHttpClient.GetEmailUpdateTokenAsync(request);
    }

    /// <summary>
    /// STEP-2: After verifying the sent otp code to the new email via EmailOtp, 
    /// finalizes updating email process with the last-given-token and new email parameters.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("update-email")]
    [Authorize]
    public async Task UpdateEmailAsync(UpdateEmailRequest request)
    {
        await _accountHttpClient.UpdateEmailAsync(request);
    }

    /// <summary>
    /// returns user information for testing RateLimiting
    /// </summary>
    /// <returns>string</returns>
    [HttpGet("RateLimitTester")]
    [EnableRateLimiting("PreventNotificationBombAttack")]
    public string ShowIpAddressAndUserId()
    {
        return string.Concat("Ip : ", _httpContextAccessor.HttpContext.Connection.RemoteIpAddress, "\n",
                             "UserId : ", _contextProvider.CurrentContext.UserId);
    }

    private async Task<bool> CheckVerificationNeededAsync(ResetPasswordOtpRequest request)
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
    
    private async Task<bool> CheckDefaultUser(ResetPasswordOtpRequest request)
    {
        var defaultUsers = _configuration.GetSection("DefaultUsers").Get<List<DefaultUser>>();
        var matchingUser = defaultUsers.FirstOrDefault(user => user.PhoneNumber == request.UserName 
                                                               && user.Otp == request.Code.ToString());

        return await Task.FromResult(matchingUser is not null);
    }
}
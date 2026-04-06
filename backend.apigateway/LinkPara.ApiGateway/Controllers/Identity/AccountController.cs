using AutoMapper;
using LinkPara.ApiGateway.Commons.Helpers;
using LinkPara.ApiGateway.Services.Emoney.HttpClients;
using LinkPara.ApiGateway.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Services.Identity.HttpClients;
using LinkPara.ApiGateway.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.Services.Identity.Models.Responses;
using LinkPara.ApiGateway.Services.Notification.HttpClients;
using LinkPara.ApiGateway.Services.Notification.Models.Enums;
using LinkPara.ApiGateway.Services.Notification.Models.Requests;
using LinkPara.ApiGateway.Services.Notification.Models.Responses;
using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace LinkPara.ApiGateway.Controllers.Identity;

public class AccountController : ApiControllerBase
{
    private readonly ILogger<AccountController> _logger;
    private readonly IAccountHttpClient _accountHttpClient;
    private readonly IOtpHttpClient _otpHttpClient;
    private readonly IUserHttpClient _userHttpClient;
    private readonly IMapper _mapper;
    private readonly IUserNameGenerator _userNameGenerator;
    private readonly IEmoneyAccountHttpClient _emoneyAccountHttpClient;
    private readonly IBus _bus;
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public AccountController(
        IAccountHttpClient accountHttpClient,
        IOtpHttpClient otpHttpClient,
        IUserHttpClient userHttpClient,
        IMapper mapper,
        IUserNameGenerator userNameGenerator,
        IEmoneyAccountHttpClient emoneyAccountHttpClient,
        IBus bus,
        ILogger<AccountController> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _accountHttpClient = accountHttpClient;
        _otpHttpClient = otpHttpClient;
        _userHttpClient = userHttpClient;
        _mapper = mapper;
        _userNameGenerator = userNameGenerator;
        _emoneyAccountHttpClient = emoneyAccountHttpClient;
        _bus = bus;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "RequireOtp")]
    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponse>> RegisterAsync(RegisterRequest request)
    {
        var requestWithUserName = _mapper.Map<RegisterWithUserName>(request);
        requestWithUserName.UserName = await _userNameGenerator.GetUserNameAsync(request.PhoneCode, request.PhoneNumber);

        var register = await _accountHttpClient.RegisterAsync(requestWithUserName);

        if (register is not null && !string.IsNullOrWhiteSpace(register.UserId.ToString()))
        {
            try
            {
                await _emoneyAccountHttpClient.CreateAccountAsync(new CreateEmoneyAccountRequest
                {
                    AccountType = AccountType.Individual,
                    AccountKycLevel = (request.ParentAccountId == Guid.Empty) ? AccountKycLevel.NoneKyc : AccountKycLevel.ChildKyc,
                    Email = request.Email,
                    PhoneCode = request.PhoneCode,
                    PhoneNumber = request.PhoneNumber,
                    IdentityUserId = Guid.Parse(register.UserId.ToString()),
                    Firstname = request.FirstName,
                    Lastname = request.LastName,
                    BirthDate = request.BirthDate,
                    ParentAccountId = request.ParentAccountId,
                    IdentityNumber = request.IdentityNumber,
                    Profession = request.Profession
                });

                var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Notification.SendIysPermission"));
                var iysPermission = new SendIysPermission
                {
                    UserId = Guid.Parse(register.UserId.ToString()),
                    IsIysApproved = request.IysPermission,
                    PhoneCode = request.PhoneCode.StartsWith("+") ? request.PhoneCode : string.Concat("+", request.PhoneCode),
                    PhoneNumber = request.PhoneNumber,
                    Email = request.Email,
                    IsCorporate = false,
                    Channel = _httpContextAccessor.HttpContext.Request.Headers["Channel"].ToString()
                };
                await endpoint.Send(iysPermission, tokenSource.Token);
            }
            catch (Exception exception)
            {
                _logger.LogError($"Account Creating Error : {exception} - UserId : {register.UserId}");

                var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Identity.DeleteUser"));
                await endpoint.Send(new DeleteUser
                {
                    UserId = Guid.Parse(register.UserId.ToString())
                }, tokenSource.Token);

                throw;
            }
        }
        return register;
    }

    /// <summary>
    /// Logins the registered user.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "RequireOtp")]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> LoginAsync(LoginRequest request)
    {
        request.Username = await _userNameGenerator.GetUserNameAsync(request.Username);

        return await _accountHttpClient.LoginAsync(request);
    }

    /// <summary>
    /// Refresh token without OTP.
    /// Last token with 'Remember Me' option is required!
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("refresh-token")]
    public async Task<ActionResult<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(request.LastToken);

        var rememberMe = bool.Parse(jwt.Claims
            .First(claim => claim.Type == "RememberMe").Value);

        request.Username = await _userNameGenerator.GetUserNameAsync(request.Username);

        return await _accountHttpClient.LoginAsync(new()
        {
            Password = request.Password,
            Username = request.Username,
            RememberMe = true
        });
    }

    [AllowAnonymous]
    [HttpPost("login-otp")]
    public async Task<SendOtpResponse> SendLoginOtpAsync(LoginRequest request)
    {
        var phone = request.Username;

        request.Username = await _userNameGenerator.GetUserNameAsync(request.Username);

        var loginResult = await _accountHttpClient.LoginAsync(request);

        var otpRequest = new SendOtpRequest()
        {
            Action = OtpAction.Login,
            OtpType = OtpType.Sms,
            Receiver = phone,
            Username = request.Username
        };

        return await _otpHttpClient.SendOtpAsync(otpRequest);
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
    [Authorize(Policy = "RequireOtp")]
    [HttpPost("reset-expired-password")]
    public async Task ResetExpiredPasswordAsync(ResetPasswordRequest request)
    {
        request.UserName = await _userNameGenerator.GetUserNameAsync(request.UserName);

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
                Receiver = phone,
                Username = request.UserName
            };

            return await _otpHttpClient.SendOtpAsync(otpRequest);
        }

        throw new InvalidOperationException();
    }

    #region ForgotPasswordFlow

    /// <summary>
    /// STEP 1
    /// Sends reset password email.
    /// </summary>
    [Authorize(Policy = "RequireOtp")]
    [HttpPost("forgot-password/email")]
    public async Task SendForgotPasswordEmailAsync(SendForgotPasswordEmailRequest request)
    {
        request.UserName = await _userNameGenerator.GetUserNameAsync(request.UserName);
        await _accountHttpClient.GetPasswordResetTokenAsync(request);
    }

    /// <summary>
    /// STEP 1
    /// Returns reset password token and email for email otp.
    /// </summary>
    [Authorize(Policy = "RequireOtp")]
    [HttpPost("forgot-password/email-otp")]
    public async Task<GetResetPasswordTokenResponse> GetResetPasswordTokenAsync(GetPasswordResetTokenAndEmailRequest request)
    {
        request.UserName = await _userNameGenerator.GetUserNameAsync(request.UserName);
        return await _accountHttpClient.GetPasswordResetTokenAndEmailAsync(request);
    }

    /// <summary>
    /// STEP 2
    /// Resets user password after identity verified.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        await _accountHttpClient.ForgotPasswordAsync(request);
    }

    #endregion

    /// <summary>
    /// STEP-1: Returns an email update token.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "User:Update")]
    [HttpGet("update-email/token")]
    public async Task<GetEmailUpdateTokenResponse> GetEmailUpdateTokenAsync([FromQuery] GetEmailUpdateTokenRequest request)
    {
        return await _accountHttpClient.GetEmailUpdateTokenAsync(request);
    }

    /// <summary>
    /// STEP-2: After verifying the sent otp code to the new email via EmailOtp, 
    /// finalizes updating email process with the last-given-token and new email parameters.
    /// </summary>
    /// <param name="request"></param>
    [Authorize(Policy = "RequireOtp")]
    [HttpPost("update-email")]
    public async Task UpdateEmailAsync(UpdateEmailRequest request)
    {
        await _accountHttpClient.UpdateEmailAsync(request);

        var account = await _emoneyAccountHttpClient.GetAccountByUserIdAsync(Guid.Parse(UserId));

        if (account is not null)
        {
            var patchRequest = new JsonPatchDocument<UpdateAccountRequest>();
            patchRequest.Replace(s => s.Email, request.NewEmail);

            await _emoneyAccountHttpClient.PatchAccountAsync(account.Id, patchRequest);
        }
        else
        {
            _logger.LogError($"Email Update Error : AccountNotFound - UserId : {UserId} ");
        }
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

    /// <summary>
    /// STEP-1: Returns an phone update token.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "User:Update")]
    [HttpGet("update-phone/token")]
    public async Task<ActionResult<GetPhoneNumberTokenResponse>> GetPhoneNumberUpdateTokenAsync([FromQuery] GetPhoneNumberUpdateTokenRequest request)
    {
        return await _accountHttpClient.GetPhoneNumberUpdateTokenAsync(request);
    }

    /// <summary>
    /// STEP-2: After verifying the send otp code to the new phone number via PhoneOtp, 
    /// finalizes updating phone number process with the last-given-token and new phone number parameters.
    /// </summary>
    /// <param name="request"></param>
    [Authorize(Policy = "RequireOtp")]
    [HttpPost("update-phone-number")]
    public async Task UpdatePhoneNumberAsync(UpdatePhoneNumberRequest request)
    {
        await _accountHttpClient.UpdatePhoneNumberAsync(request);

        var account = await _emoneyAccountHttpClient.GetAccountByUserIdAsync(Guid.Parse(UserId));

        if (account is not null)
        {
            var patchRequest = new JsonPatchDocument<UpdateAccountRequest>();
            patchRequest.Replace(s => s.PhoneNumber, request.NewPhoneNumber);
            patchRequest.Replace(s => s.PhoneCode, request.NewPhoneCode);

            await _emoneyAccountHttpClient.PatchAccountAsync(account.Id, patchRequest);
        }
        else
        {
            _logger.LogError($"Update Phone Number Error : AccountNotFound - UserId : {UserId} ");
        }
    }
}
using LinkPara.ApiGateway.Services.Identity.HttpClients;
using LinkPara.ApiGateway.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.Services.Notification.HttpClients;
using LinkPara.ApiGateway.Services.Notification.Models.Enums;
using LinkPara.ApiGateway.Services.Notification.Models.Requests;
using LinkPara.ApiGateway.Services.Notification.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LinkPara.SharedModels.Exceptions;
using LinkPara.ApiGateway.Commons.IdentityModels;
using LinkPara.ApiGateway.Commons.Helpers;
using LinkPara.HttpProviders.Identity;

namespace LinkPara.ApiGateway.Controllers.Notification;

public class OtpController : ApiControllerBase
{
    private readonly IOtpHttpClient _otpHttpClient;
    private readonly IUserHttpClient _userHttpClient;
    private readonly IUserNameGenerator _userNameGenerator;
    private readonly IRecaptchaService _recaptchaService;

    public OtpController(IOtpHttpClient otpHttpClient,
        IUserHttpClient userHttpClient,
        IUserNameGenerator userNameGenerator,
        IRecaptchaService recaptchaService)
    {
        _otpHttpClient = otpHttpClient;
        _userHttpClient = userHttpClient;
        _userNameGenerator = userNameGenerator;
        _recaptchaService = recaptchaService;
    }

    /// <summary>
    /// Sends sms or email to verify the user 
    /// OtpTypes : 0-Sms / 1-Email
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("")]
    public async Task<ActionResult<SendOtpResponse>> SendOtpAsync(SendOtpRequest request)
    {
        
        if (request.Action == OtpAction.Register)
        {
            if (request.OtpType == OtpType.Email)
            {
                await _recaptchaService.VerifyAsync(request.RecaptchaToken);
            }

            var userFilterRequest = request.OtpType switch
            {
                OtpType.Email => new GetExistingUsersRequest { Email = request.Receiver },
                OtpType.Sms => new GetExistingUsersRequest { UserName = await _userNameGenerator.GetUserNameAsync(request.Receiver) },
                _ => throw new InvalidParameterException(nameof(OtpType))
            };

            var verifyReceiver = await _userHttpClient.GetExistingUserListAsync(userFilterRequest);

            if (verifyReceiver.IsExists)
            {
                var individualUser = verifyReceiver.UserNames.Where(s => s.StartsWith(UserTypePrefix.Individual));
                if (individualUser.Any())
                {
                    throw new DuplicateRecordException();
                }
            }
        }

        if (request.Action == OtpAction.ForgotPassword)
        {
            await _recaptchaService.VerifyAsync(request.RecaptchaToken);
            if (request.OtpType == OtpType.Sms)
            {
                try
                {
                    var userName = await _userNameGenerator.GetUserNameAsync(request.Receiver);
                    var trimedUserName = userName.Replace(" ", "");

                    if (!String.IsNullOrEmpty(trimedUserName))
                    {
                        var isUserExist = await _userHttpClient.GetIsUserExistAsync(trimedUserName);

                        if (!isUserExist)
                        {
                            return new SendOtpResponse
                            {
                                IsSuccess = true,
                                ExpirationDate = DateTime.Now.AddMinutes(3),
                                OtpAuthorizationId = Guid.NewGuid().ToString()
                            };
                        }
                    }
                }
                catch (Exception)
                {
                    return new SendOtpResponse
                    {
                        IsSuccess = true,
                        ExpirationDate = DateTime.Now.AddMinutes(3),
                        OtpAuthorizationId = Guid.NewGuid().ToString()
                    };
                }
            }          
        }

        return await _otpHttpClient.SendOtpAsync(request);
    }

    /// <summary>
    /// Sends sms or email to logged in user
    /// OtpTypes : 0-Sms / 1-Email
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize]
    [HttpPost("me")]
    public async Task<ActionResult<SendOtpResponse>> SendOtpToMeAsync(SendOtpRequestToMember request)
    {
        var user = await _userHttpClient.GetUserAsync(Guid.Parse(UserId));

        if (user == null)
        {
            return new ForbidResult();
        }

        return await _otpHttpClient.SendOtpAsync(new SendOtpRequest
        {
            Action = request.Action,
            Receiver = request.OtpType == OtpType.Email
                ? user.Email
                : user.PhoneNumber,
            OtpType = request.OtpType
        });
    }

    /// <summary>
    /// Verifies the submitted otp code
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("verify")]
    public async Task<ActionResult<VerifyOtpResponse>> VerifyOtpAsync([FromQuery] VerifyOtpRequest request)
    {
        return await _otpHttpClient.VerifyOtpAsync(request);
    }

    /// <summary>
    /// Verifies the submitted otp code
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("~/v2/Otp/verify")]
    public async Task<ActionResult<VerifyOtpResponse>> VerifyOtpV2Async([FromBody] VerifyOtpRequest request)
    {
        return await _otpHttpClient.VerifyOtpAsync(request);
    }
}
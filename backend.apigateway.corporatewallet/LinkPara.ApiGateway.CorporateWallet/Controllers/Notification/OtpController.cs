using LinkPara.ApiGateway.CorporateWallet.Commons.Helpers;
using LinkPara.ApiGateway.CorporateWallet.Commons.Models.IdentityModels;
using LinkPara.ApiGateway.CorporateWallet.Services.Identity.HttpClients;
using LinkPara.ApiGateway.CorporateWallet.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.Notification.HttpClients;
using LinkPara.ApiGateway.CorporateWallet.Services.Notification.Models.Enums;
using LinkPara.ApiGateway.CorporateWallet.Services.Notification.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.Notification.Models.Responses;
using LinkPara.HttpProviders.Identity;
using LinkPara.SharedModels.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.CorporateWallet.Controllers.Notification;

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
                var businessUser = verifyReceiver.UserNames.Where(s => s.StartsWith(UserTypePrefix.CorporateWallet));
                if (businessUser.Any())
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
                        var trimedUserNameDto = new GetExistingUsersRequest { UserName = trimedUserName };
                        var isUserExist = await _userHttpClient.GetExistingUserListAsync(trimedUserNameDto);

                        if (isUserExist is not null && !isUserExist.IsExists)
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
    /// Verifies the submitted otp code
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("verify")]
    public async Task<ActionResult<VerifyOtpResponse>> VerifyOtpAsync([FromQuery] VerifyOtpRequest request)
    {
        return await _otpHttpClient.VerifyOtpAsync(request);
    }
}

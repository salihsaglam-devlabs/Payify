using LinkPara.Identity.Application.Common.Models.AccountModels;
using LinkPara.Identity.Application.Features.Account.Commands.ForgotPassword;
using LinkPara.Identity.Application.Features.Account.Commands.Register;
using LinkPara.Identity.Application.Features.Account.Commands.RegisterWithCustomer;
using LinkPara.Identity.Application.Features.Account.Commands.ResetPassword;
using LinkPara.Identity.Application.Features.Account.Commands.SendPasswordResetToken;
using LinkPara.Identity.Application.Features.Account.Commands.SendResetPasswordToken;
using LinkPara.Identity.Application.Features.Account.Commands.UpdateEmail;
using LinkPara.Identity.Application.Features.Account.Commands.UpdatePhoneNumber;
using LinkPara.Identity.Application.Features.Account.Queries;
using LinkPara.Identity.Application.Features.Account.Queries.CheckBirthdateAllowedRange;
using LinkPara.Identity.Application.Features.Account.Queries.CheckUserInformation;
using LinkPara.Identity.Application.Features.Account.Queries.GetEmailUpdateToken;
using LinkPara.Identity.Application.Features.Account.Queries.GetPhoneNumberUpdateToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Identity.API.Controllers;

public class AccountController : ApiControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponse>> RegisterAsync(RegisterCommand command)
    {
        return await Mediator.Send(command);
    }

    /// <summary>
    /// register with external customer & person id
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("register-with-customer")]
    public async Task<ActionResult<RegisterResponse>> RegisterWithCustomerAsync(RegisterWithCustomerCommand command)
    {
        return await Mediator.Send(command);
    }

    /// <summary>
    /// Reset password operation for logged in users.
    /// </summary>
    /// <param name="command"></param>
    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task ResetPasswordAsync(ResetPasswordCommand command)
    {
        await Mediator.Send(command);
    }    

    /// <summary>
    /// STEP 1
    /// Sends reset password email.
    /// </summary>
    /// <param name="command"></param>
    [AllowAnonymous]
    [HttpPost("forgot-password/email")]
    public async Task<string> SendPasswordResetTokenAsync(SendPasswordResetTokenCommand command)
    {
        return await Mediator.Send(command);
    }

    /// <summary>
    /// STEP 1
    /// Returns reset password token and email.
    /// </summary>
    /// <param name="command"></param>
    [AllowAnonymous]
    [HttpPost("forgot-password/email-otp")]
    public async Task<ResetPasswordTokenResponse> SendPasswordResetTokenAsync(SendResetPasswordTokenCommand command)
    {
        return await Mediator.Send(command);
    }

    /// <summary>
    /// STEP 2
    /// Resets user password after identity verified.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task ForgotPasswordAsync(ForgotPasswordCommand command)
    {
        await Mediator.Send(command);
    }
    
    /// <summary>
    /// STEP-1: Returns an email update token.
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "User:Update")]
    [HttpGet("update-email/token")]
    public async Task<ActionResult<UpdateEmailTokenDto>> GetEmailUpdateTokenAsync([FromQuery] GetEmailUpdateTokenQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// STEP-2: After verifying the sent otp code to the new email via EmailOtp, 
    /// finalizes updating email process with the last-given-token and new email parameters.
    /// </summary>
    /// <param name="command"></param>
    [AllowAnonymous]
    [HttpPost("update-email")]
    public async Task UpdateEmailAsync(UpdateEmailCommand command)
    {
        await Mediator.Send(command);
    }
    
    /// <summary>
    /// Checks if the date of birth is valid.
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("check-birthdate")]
    public async Task<bool> CheckBirthdateAllowedRange([FromQuery] CheckBirthdateAllowedRangeQuery query)
    {
        return await Mediator.Send(query);
    }

    [AllowAnonymous]
    [HttpPost("check-user-for-register")]
    public async Task<bool> CheckUserForRegister(CheckUserForRegisterQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// STEP-1: Returns an phone update token.
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "User:Update")]
    [HttpGet("update-phone/token")]
    public async Task<ActionResult<UpdatePhoneNumberTokenDto>> GetPhoneNumberUpdateTokenAsync([FromQuery] GetPhoneNumberUpdateTokenQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// STEP-2: After verifying the send otp code to the new phone number via PhoneOtp, 
    /// finalizes updating phone number process with the last-given-token and new phone number parameters.
    /// </summary>
    /// <param name="command"></param>
    [AllowAnonymous]
    [HttpPost("update-phone-number")]
    public async Task UpdatePhoneNumberAsync(UpdatePhoneNumberCommand command)
    {
        await Mediator.Send(command);
    }
}
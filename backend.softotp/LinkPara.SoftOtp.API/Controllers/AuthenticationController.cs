using LinkPara.SoftOtp.Application.Common.Models.PowerFactorModels.Response;
using LinkPara.SoftOtp.Application.Features.Auth.Commands.DeviceActivation;
using LinkPara.SoftOtp.Application.Features.Auth.Commands.UpdateActivationPINByCustomerIdTransaction;
using LinkPara.SoftOtp.Application.Features.Auth.Commands.VerifyLogin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.SoftOtp.API.Controllers;

public class AuthenticationController : ApiControllerBase
{
    [AllowAnonymous]
    [HttpPost("device-activation")]
    public async Task<GenerateActivationOtpResponse> MultifactorActivationAsync(MultifactorAuthCommand command)
    {
        return await Mediator.Send(command);
    }

    [AllowAnonymous]
    [HttpPost("verify-login")]
    public async Task<VerifyLoginOtpResponse> VerifyLoginOtp(VerifyLoginCommand command)
    {
        return await Mediator.Send(command);
    }

    [AllowAnonymous]
    [HttpPost("update-activation-pin")]
    public async Task<UpdateActivationPINByCustomerIdResponse> UpdateActivationPINByCustomerId(UpdateActivationPINByCustomerIdCommand command)
    {
        return await Mediator.Send(command);
    }
}
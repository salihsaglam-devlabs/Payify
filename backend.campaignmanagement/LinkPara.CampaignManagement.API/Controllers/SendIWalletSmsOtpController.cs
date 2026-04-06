using LinkPara.CampaignManagement.Application.Features.IWalletOtpCodes;
using LinkPara.CampaignManagement.Application.Features.IWalletOtpCodes.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.CampaignManagement.API.Controllers;

public class SendIWalletSmsOtpController : ApiControllerBase
{
    /// <summary>
    /// SendIWalletSmsOtp
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("")]
    public async Task<SendIWalletOtpCodeResponseDto> SendIWalletSmsOtpAsync(SendIWalletOtpCodeCommand command)
    {
        return await Mediator.Send(command);
    }
}

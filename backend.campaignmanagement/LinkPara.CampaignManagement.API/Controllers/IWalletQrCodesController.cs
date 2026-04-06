using LinkPara.CampaignManagement.Application.Features.IWalletQrCodes;
using LinkPara.CampaignManagement.Application.Features.IWalletQrCodes.Commands.CreateQrCode;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.CampaignManagement.API.Controllers;

public class IWalletQrCodesController : ApiControllerBase
{
    /// <summary>
    /// Generate QrCode
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "IWalletQrCode:Create")]
    [HttpPost("")]
    public async Task<IWalletQrCodeDto> GenerateQrCodeAsync(CreateQrCodeCommand request)
    {
        return await Mediator.Send(request);
    }

}

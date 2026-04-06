using Microsoft.AspNetCore.Mvc;
using LinkPara.CampaignManagement.Application.Features.IWalletCards.Commands;
using MediatR;
using LinkPara.CampaignManagement.Application.Features.IWalletCards.Queries.GetCard;
using LinkPara.CampaignManagement.Application.Features.IWalletCards;
using LinkPara.CampaignManagement.Application.Features.IWalletQrCodes;
using Microsoft.AspNetCore.Authorization;

namespace LinkPara.CampaignManagement.API.Controllers;

public class IWalletCardsController : ApiControllerBase
{
    /// <summary>
    /// Create Card
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "IWalletCard:Create")]
    [HttpPost("")]
    public async Task<IWalletQrCodeDto> CreateCardAsync(CreateCardCommand request)
    {
        return await Mediator.Send(request);
    }

    /// <summary>
    /// Get Card Info
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "IWalletCard:Read")]
    [HttpGet("")]
    public async Task<IWalletCardDto> GetCardAsync([FromQuery]GetCardQuery request)
    {
        return await Mediator.Send(request);
    }
}

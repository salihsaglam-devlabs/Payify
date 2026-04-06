using LinkPara.CampaignManagement.Application.Features.IWalletCashbacks.Commands.CashBack;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.CampaignManagement.API.Controllers;

public class IWalletCashBackController : ApiControllerBase
{
    /// <summary>
    /// CashBack
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("")]
    public async Task CashBackAsync(CashBackCommand command)
    {
        await Mediator.Send(command);
    }
}

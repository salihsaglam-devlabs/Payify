using LinkPara.CampaignManagement.Application.Features.IWalletLocations.Queries.GetTowns;
using LinkPara.CampaignManagement.Application.Features.IWalletLocations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LinkPara.CampaignManagement.Application.Features.IWalletLogins;
using LinkPara.CampaignManagement.Application.Features.IWalletLogins.Commands.Login;

namespace LinkPara.CampaignManagement.API.Controllers;

public class IWalletLoginController : ApiControllerBase
{
    /// <summary>
    /// Login
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("")]
    public async Task<LoginResponseDto> LoginAsync(LoginCommand command)
    {
        return await Mediator.Send(command);
    }
}

using LinkPara.Emoney.Application.Features.PayWithWallets.Commands.Transfer;
using LinkPara.Emoney.Application.Features.PayWithWallets.Commands.TransferForLoggedInUser;
using LinkPara.Emoney.Application.Features.PayWithWallets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Emoney.API.Controllers;

public class PayWithWalletsController : ApiControllerBase
{
    /// <summary>
    /// This method used to money transfer to partner.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "PayWithWallet:Create")]
    [HttpPost("transfer")]
    public async Task<PayWithWalletResponse> PayWithWalletAsync(PayWithWalletCommand command)
    {
        return await Mediator.Send(command);
    }

    /// <summary>
    /// This method used to money transfer for logged in user.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "PayWithWallet:Create")]
    [HttpPost("transfer-for-logged-in-user")]
    public async Task<PayWithWalletResponse> TransferForLoggedInUserAsync(TransferForLoggedInUserCommand command)
    {
        return await Mediator.Send(command);
    }
}

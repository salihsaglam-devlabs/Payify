using LinkPara.CampaignManagement.Application.Features.IWalletCharges.Commands.ReverseCharge;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.CampaignManagement.API.Controllers
{
    public class IWalletReverseChargeController: ApiControllerBase
    {
        /// <summary>
        /// Reverse Charge
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("")]
        public async Task ReverseChargeByIWalletAsync(ReverseChargeCommand command)
        {
            await Mediator.Send(command);
        }
    }
}

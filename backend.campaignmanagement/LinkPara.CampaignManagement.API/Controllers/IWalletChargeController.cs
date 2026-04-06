using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LinkPara.CampaignManagement.Application.Features.IWalletCharges.Commands.ChargeByIWallet;
using LinkPara.CampaignManagement.Application.Features.IWalletCharges.Queries.GetCharges;
using LinkPara.CampaignManagement.Application.Features.IWalletCharges;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.CampaignManagement.API.Controllers;

public class IWalletChargeController : ApiControllerBase
{
    /// <summary>
    /// Charge
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("")]
    public async Task<Guid> ChargeByIWalletAsync(ChargeByIWalletCommand command)
    {
        return await Mediator.Send(command);
    }

    /// <summary>
    /// Charge
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "IWalletCharge:ReadAll")]
    [HttpGet("")]
    public async Task<PaginatedList<ChargeTransactionDto>> ChargeByIWalletAsync([FromQuery]GetChargeTransactionsSearchQuery query)
    {
        return await Mediator.Send(query);
    }
}

using LinkPara.PF.Application.Features.MerchantBlockages;
using LinkPara.PF.Application.Features.MerchantBlockages.Command.SaveMerchantBlockage;
using LinkPara.PF.Application.Features.MerchantBlockages.Queries.GetMerchantBlockageByMerchantId;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Mvc;
using LinkPara.PF.Application.Features.MerchantBlockages.Queries.GetAllMerchantBlockages;
using LinkPara.PF.Application.Features.MerchantBlockages.Command.UpdateMerchantBlockage;
using LinkPara.PF.Application.Features.MerchantBlockages.Command.UpdatePaymentDate;
using Microsoft.AspNetCore.Authorization;

namespace LinkPara.PF.API.Controllers;

public class MerchantBlockageController : ApiControllerBase
{
    /// <summary>
    /// Returns all merchant blockage
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "MerchantBlockage:ReadAll")]
    public async Task<ActionResult<PaginatedList<MerchantBlockageDto>>> GetTimeoutTransactionsAsync([FromQuery] GetAllMerchantBlockageQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Returns merchant blockages
    /// </summary>
    /// <param name="merchantId"></param>
    /// <returns></returns>
    [HttpGet("{merchantId}")]
    [Authorize(Policy = "MerchantBlockage:Read")]
    public async Task<ActionResult<MerchantBlockageDto>> GetMerchantBlockageByMerchantId([FromRoute] Guid merchantId)
    {
        return await Mediator.Send(new GetMerchantBlockageByMerchantIdQuery { MerchantId = merchantId });
    }

    /// <summary>
    /// Create new blockage for merchant
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantBlockage:Create")]
    [HttpPost("")]
    public async Task SaveAsync(SaveMerchantBlockageCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Updates a total amount
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantBlockage:Update")]
    [HttpPut("")]
    public async Task UpdateAsync(UpdateMerchantBlockageCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Updates a payment date 
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantBlockage:Update")]
    [HttpPut("payment-date")]
    public async Task UpdateAsync(UpdatePaymentDateCommand command)
    {
        await Mediator.Send(command);
    }
}
using LinkPara.Emoney.Application.Commons.Models.ProvisionModels;
using LinkPara.Emoney.Application.Features.Provisions.Commands.Provision;
using LinkPara.Emoney.Application.Features.Wallets.Commands.CancelProvision;
using Microsoft.AspNetCore.Mvc;
using LinkPara.Emoney.Application.Features.Provisions.Queries.ProvisionPreview;
using LinkPara.Emoney.Application.Features.Provisions.Queries.InquireProvision;
using LinkPara.Emoney.Application.Features.Provisions;
using Microsoft.AspNetCore.Authorization;
using LinkPara.Emoney.Application.Features.Provisions.Commands.ProvisionCashback;
using LinkPara.Emoney.Application.Features.Provisions.Commands.CancelProvisionCashback;
using LinkPara.Emoney.Application.Features.Provisions.Commands.ReturnProvision;

namespace LinkPara.Emoney.API.Controllers;

public class ProvisionsController : ApiControllerBase
{
    /// <summary>
    /// Provision from wallet.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "Provision:Create")]
    [HttpPost("")]
    public async Task<ProvisionResponse> ProvisionAsync(ProvisionCommand command)
    {
        return await Mediator.Send(command);
    }

    /// <summary>
    /// cancel existing provision
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "Provision:Update")]
    [HttpPut("")]
    public async Task<ProvisionResponse> CancelProvisionAsync(CancelProvisionCommand command)
    {
        return await Mediator.Send(command);
    }

    /// <summary>
    /// Provision preview
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "Provision:Read")]
    [HttpGet("")]
    public async Task<ProvisionPreviewResponse> ProvisionPreviewAsync([FromQuery] ProvisionPreviewQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// partner provision status
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "Provision:Read")]
    [HttpGet("inquire")]
    public async Task<InquireProvisionResponse> InquireProvisionAsync([FromQuery] InquireProvisionQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// ProvisionCashback to wallet.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "Provision:Create")]
    [HttpPost("cashback")]
    public async Task<ProvisionCashbackResponse> ProvisionCashbackAsync(ProvisionCashbackCommand command)
    {
        return await Mediator.Send(command);
    }

    /// <summary>
    /// CancelProvisionCashback from wallet
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "Provision:Update")]
    [HttpPut("cancel-cashback")]
    public async Task<ProvisionCashbackResponse> CancelProvisionCashbackAsync(CancelProvisionCashbackCommand command)
    {
        return await Mediator.Send(command);
    }

    /// <summary>
    /// Partially Return to wallet.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "Provision:Create")]
    [HttpPost("partially-return")]
    public async Task<ProvisionResponse> ReturnProvisionAsync(ReturnProvisionCommand command)
    {
        return await Mediator.Send(command);
    }

    /// <summary>
    /// ProvisionChargeback to wallet.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "Provision:Create")]
    [HttpPost("chargeback")]
    public async Task<ProvisionChargebackResponse> ProvisionChargebackAsync(ProvisionChargebackCommand command)
    {
        return await Mediator.Send(command);
    }

}
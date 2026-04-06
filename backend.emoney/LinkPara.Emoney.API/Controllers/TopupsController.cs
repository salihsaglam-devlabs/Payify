using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models.PaymentProviderModels.PayifyPf.Requests;
using LinkPara.Emoney.Application.Commons.Models.PaymentProviderModels.PayifyPf.Responses;
using LinkPara.Emoney.Application.Features.Topups;
using LinkPara.Emoney.Application.Features.Topups.Commands.TopupCancel;
using LinkPara.Emoney.Application.Features.Topups.Commands.TopupProcess;
using LinkPara.Emoney.Application.Features.Topups.Commands.TopupReturnToWallet;
using LinkPara.Emoney.Application.Features.Topups.Commands.TopupUpdateStatus;
using LinkPara.Emoney.Application.Features.Topups.Queries.GetTopupPreview;
using LinkPara.Emoney.Application.Features.Topups.Queries.GetTopups;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.HttpProviders.Vault;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Emoney.API.Controllers;

public class TopupsController : ApiControllerBase
{
    private readonly IPaymentProviderServiceFactory _paymentServiceFactory;
    private readonly IVaultClient _vaultClient;
    private readonly string paymentProviderType;

    public TopupsController(IPaymentProviderServiceFactory paymentServiceFactory,
        IVaultClient vaultClient)
    {
        _paymentServiceFactory = paymentServiceFactory;
        _vaultClient = vaultClient;
        paymentProviderType = _vaultClient.GetSecretValue<string>("SharedSecrets", "PaymentProviderConfigs", "Type");
    }

    /// <summary>
    /// Topup preview 
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "Topup:Read")]
    [HttpGet("preview")]
    public async Task<TopupPreviewResponse> TopupPreviewAsync([FromQuery] GetTopupPreviewQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Topup process
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "Topup:Create")]
    [HttpPost("process")]
    public async Task<TopupProcessResponse> TopupProcessAsync(TopupProcessRequest request)
    {
        var command = new TopupProcessCommand
        {
            BaseRequest = request
        };

        return await Mediator.Send(command);
    }

    /// <summary>
    /// Topup cancel
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "Topup:Create")]
    [HttpPost("cancel")]
    public async Task<TopupCancelResponse> TopupCancelAsync(TopupCancelRequest request)
    {
        var command = new TopupCancelCommand
        {
            BaseRequest = request
        };

        return await Mediator.Send(command);
    }

    /// <summary>
    /// Topup list
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "Topup:ReadAll")]
    [HttpGet("")]
    public async Task<PaginatedList<TopupDto>> TopupListAsync([FromQuery] GetTopupListQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Topup update status
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "Topup:Update")]
    [HttpPut("update-status")]
    public async Task TopupUpdateStatusAsync(TopupUpdateStatusCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Topup return to wallet
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "Topup:Create")]
    [HttpPost("return-to-wallet")]
    public async Task TopupReturnToWalletAsync(TopupReturnToWalletCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Get 3D Session
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Topup:Create")]
    [HttpPost("getthreedsession")]
    public async Task<GetThreeDSessionResponse> GetThreeDSessionAsync(GetThreeDSessionRequest request)
    {
        var paymentProviderService = await _paymentServiceFactory.GetPaymentProviderServiceAsync(paymentProviderType);
        return await paymentProviderService.GetThreeDSessionAsync(request);
    }

    /// <summary>
    /// Get 3D Session Result
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Topup:Create")]
    [HttpPost("getthreedsessionresult")]
    public async Task<GetThreeDSessionResultResponse> GetThreeDSessionResultAsync([FromBody] GetThreeDSessionResultRequest request)
    {
        var paymentProviderService = await _paymentServiceFactory.GetPaymentProviderServiceAsync(paymentProviderType);
        return await paymentProviderService.GetThreeDSessionResultAsync(request);
    }

    /// <summary>
    /// Provision
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Topup:Create")]
    [HttpPost("provision")]
    public async Task<PaymentProviderProvisionResponse> ProvisionAsync([FromBody] PaymentProviderProvisionRequest request)
    {
        var paymentProviderService = await _paymentServiceFactory.GetPaymentProviderServiceAsync(paymentProviderType);
        return await paymentProviderService.ProvisionAsync(request);
    }

    /// <summary>
    /// Reverse
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Topup:Create")]
    [HttpPost("reverse")]
    public async Task<ReverseResponse> ReverseAsync([FromBody] ReverseRequest request)
    {
        var paymentProviderService = await _paymentServiceFactory.GetPaymentProviderServiceAsync(paymentProviderType);
        return await paymentProviderService.ReverseAsync(request);
    }

    /// <summary>
    /// Return
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Topup:Create")]
    [HttpPost("return")]
    public async Task<ReturnResponse> ReturnAsync([FromBody] ReturnRequest request)
    {
        var paymentProviderService = await _paymentServiceFactory.GetPaymentProviderServiceAsync(paymentProviderType);
        return await paymentProviderService.ReturnAsync(request);
    }

    /// <summary>
    /// Generate card token for PayifyPf
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Topup:Create")]
    [HttpPost("token")]
    public async Task<CardTokenDto> GenerateCardTokenAsync(
        [FromBody] GenerateCardTokenRequest request)
    {
        request.PaymentProviderType = Enum.Parse<PaymentProviderType>(paymentProviderType);
        var paymentProviderService = await _paymentServiceFactory.GetPaymentProviderServiceAsync(paymentProviderType);

        return await paymentProviderService.GenerateCardTokenAsync(request);
    }

    /// <summary>
    /// Init 3ds 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Topup:Create")]
    [HttpPost("init3ds")]
    public async Task<Init3dsResponse> Init3dsAsync([FromBody] Init3dsRequest request)
    {
        var paymentProviderService = await _paymentServiceFactory.GetPaymentProviderServiceAsync(paymentProviderType);
        return await paymentProviderService.Init3dsAsync(request);
    }
}
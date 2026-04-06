using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Features.Topups.Commands.TopupProcess;
using LinkPara.Emoney.Application.Features.Topups;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LinkPara.Emoney.Application.Commons.Models.Masterpass.Requests;
using LinkPara.Emoney.Application.Commons.Models.Masterpass.Responses;
using LinkPara.Emoney.Application.Features.Topups.Commands.TopupCancel;

namespace LinkPara.Emoney.API.Controllers;

public class MasterpassController : ApiControllerBase
{
    private readonly IMasterpassService _masterpassService;

    public MasterpassController(IMasterpassService masterpassService)
        => _masterpassService = masterpassService;

    /// <summary>
    /// Generate access token
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Topup:Create")]
    [HttpPost("access-token")]
    public async Task<BaseResponse<GenerateAccessTokenResponse>> GenerateAccessTokenAsync([FromBody] GenerateAccessTokenRequest request)
        => await _masterpassService.GenerateAccessTokenAsync(request);

    /// <summary>
    /// Validate threed secure
    /// </summary>
    /// <param name="orderId"></param>
    /// <returns></returns>
    [Authorize(Policy = "Topup:Read")]
    [HttpGet("validate-threed-secure")]
    public async Task<ValidateThreedSecureResponse> ValidateThreedSecureAsync(string orderId)
        => await _masterpassService.ValidateThreedSecureAsync(orderId);

    /// <summary>
    /// Threed secure
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("threed-secure")]
    public async Task ThreedSecureAsync([FromBody] ThreedSecureRequest request)
        => await _masterpassService.ThreedSecureAsync(request);

    /// <summary>
    /// Account unlink
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Topup:Create")]
    [HttpPost("account-unlink")]
    public async Task<BaseResponse<AccountUnlinkResponse>> AccountUnlinkAsync([FromBody] AccountUnlinkRequest request)
        => await _masterpassService.AccountUnlinkAccountAsync(request);

    /// <summary>
    /// Topup process
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "Topup:Create")]
    [HttpPost("process")]
    public async Task<TopupProcessResponse> TopupProcessAsync(MasterpassTopupProcessRequest request)
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
    public async Task<TopupCancelResponse> TopupCancelAsync(MasterpassTopupCancelRequest request)
    {
        var command = new TopupCancelCommand
        {
            BaseRequest = request
        };

        return await Mediator.Send(command);
    }
}
using LinkPara.ApiGateway.Services.Emoney.HttpClients;
using LinkPara.ApiGateway.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Services.Emoney.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Controllers.Emoney;

public class MasterpassController : ApiControllerBase
{
    private readonly IMasterpassHttpClient _masterpassHttpClient;

    public MasterpassController(IMasterpassHttpClient masterpassHttpClient)
        => _masterpassHttpClient = masterpassHttpClient;

    /// <summary>
    /// Generate access token
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Topup:Create")]
    [HttpPost("access-token")]
    public async Task<BaseResponse<GenerateAccessTokenResponse>> GenerateAccessTokenAsync([FromBody] GenerateAccessTokenRequest request)
        => await _masterpassHttpClient.GenerateAccessTokenAsync(request);

    /// <summary>
    /// Validate threed secure
    /// </summary>
    /// <param name="orderId"></param>
    /// <returns></returns>
    [Authorize(Policy = "Topup:Read")]
    [HttpGet("validate-threed-secure")]
    public async Task<ValidateThreedSecureResponse> ValidateThreedSecureAsync(string orderId)
        => await _masterpassHttpClient.ValidateThreedSecureAsync(orderId);

    /// <summary>
    /// Threed secure
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("threed-secure")]
    public async Task ThreedSecureAsync([FromForm] ThreedSecureRequest request)
        => await _masterpassHttpClient.ThreedSecureAsync(request);

    /// <summary>
    /// Account unlink
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Topup:Create")]
    [HttpPost("account-unlink")]
    public async Task<BaseResponse<AccountUnlinkResponse>> AccountUnlinkAsync([FromBody] AccountUnlinkRequest request)
        => await _masterpassHttpClient.AccountUnlinkAccountAsync(request);

    /// <summary>
    /// Topup process
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "Topup:Create")]
    [HttpPost("process")]
    public async Task<TopupProcessResponse> TopupProcessAsync(MasterpassTopupProcessRequest request)
        => await _masterpassHttpClient.TopupProcessAsync(request);
}

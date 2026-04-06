using LinkPara.ApiGateway.Boa.Filters.CustomerContext;
using LinkPara.ApiGateway.Boa.Services.Emoney.HttpClients;
using LinkPara.ApiGateway.Boa.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Boa.Services.Emoney.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Boa.Controllers.Emoney;

public class TopupsController : ApiControllerBase
{
    private readonly ITopupHttpClient _topupHttpClient;

    public TopupsController(ITopupHttpClient topupHttpClient)
        => _topupHttpClient = topupHttpClient;

    /// <summary>
    /// Returns topup preview
    /// </summary>
    /// <returns></returns>
    [HttpGet("preview")]
    [CustomerContextRequired]
    public async Task<TopupPreviewResponse> TopupPreviewAsync([FromQuery] TopupPreviewRequest request)
    {
        return await _topupHttpClient.GetTopupPreviewAsync(request);
    }

    /// <summary>
    /// Topup process
    /// </summary>
    /// <returns></returns>
    [HttpPost("process")]
    [CustomerContextRequired]
    public async Task<TopupProcessResponse> TopupProcessAsync(TopupProcessRequest request)
        => await _topupHttpClient.TopupProcessAsync(request);

    /// <summary>
    /// Get 3D Session
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("getthreedsession")]
    [CustomerContextRequired]
    public async Task<GetThreeDSessionResponse> GetThreeDSessionAsync(GetThreeDSessionRequest request)
        => await _topupHttpClient.GetThreeDSessionAsync(request);

    /// <summary>
    /// Get 3D Session Result
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("getthreedsessionresult")]
    [CustomerContextRequired]
    public async Task<GetThreeDSessionResultResponse> GetThreeDSessionResultAsync(GetThreeDSessionResultRequest request)
        => await _topupHttpClient.GetThreeDSessionResultAsync(request);

    /// <summary>
    /// Generate card token
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("token")]
    [CustomerContextRequired]
    public async Task<CardTokenResponse> GenerateCardTokenAsync(GenerateCardTokenRequest request)
        => await _topupHttpClient.GenerateCardTokenAsync(request);

    /// <summary>
    /// Init 3ds
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("init3ds")]
    [CustomerContextRequired]
    public async Task<Init3dsResponse> Init3dsAsync(Init3dsRequest request)
       => await _topupHttpClient.Init3dsAsync(request);
}

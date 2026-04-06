using LinkPara.ApiGateway.Services.DigitalKyc.HttpClients;
using LinkPara.ApiGateway.Services.DigitalKyc.Models.Requests.ScSoft;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Controllers.DigitalKyc;
public class ScSoftController : ApiControllerBase
{
    private readonly IScSoftHttpClient _httpClient;

    public ScSoftController(IScSoftHttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    /// <summary>
    /// Checks id and starts the session of Scsoft.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "DigitalKyc:Create")]
    [HttpPost("check-id")]
    public async Task<ScSoftAtapiResponse> CheckIdentityIsNewAsync(CheckIdentityIsNewRequest request)
    {
        return await _httpClient.CheckIdentityIsNewAsync(request);
    }

    /// <summary>
    /// Checks kps informations.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "DigitalKyc:Create")]
    [HttpPost("check-kps")]
    public async Task<ScSoftAtapiResponse> CheckKpsInformationsAsync(CheckKpsInformationsRequest request)
    {
        return await _httpClient.CheckKpsInformationsAsync(request);
    }

    /// <summary>
    /// Checks front id informations.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "DigitalKyc:Create")]
    [HttpPost("check-front")]
    public async Task<ScSoftAtapiResponse> CheckFrontIdentityAsync(CheckFrontIdentityRequest request)
    {
        return await _httpClient.CheckFrontIdentityAsync(request);
    }

    /// <summary>
    /// Checks rear id informations.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "DigitalKyc:Create")]
    [HttpPost("check-rear")]
    public async Task<ScSoftAtapiResponse> CheckRearIdentityAsync(CheckRearIdentityRequest request)
    {
        return await _httpClient.CheckRearIdentityAsync(request);
    }

    /// <summary>
    /// Checks nfc informations.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "DigitalKyc:Create")]
    [HttpPost("check-nfc")]
    public async Task<ScSoftAtapiResponse> CheckNfcAsync(CheckNfcRequest request)
    {
        return await _httpClient.CheckNfcAsync(request);
    }

    /// <summary>
    /// Checks head pose.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "DigitalKyc:Create")]
    [HttpPost("check-headpose")]
    public async Task<ScSoftAtapiResponse> CheckHeadPoseAsync(CheckHeadPoseRequest request)
    {
        return await _httpClient.CheckHeadPoseAsync(request);
    }

    /// <summary>
    /// Checks spoof.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "DigitalKyc:Create")]
    [HttpPost("check-spoof")]
    public async Task<ScSoftAtapiResponse> CheckSpoofAsync(CheckSpoofRequest request)
    {
        return await _httpClient.CheckSpoofAsync(request);
    }

    /// <summary>
    /// Checks defined similarity rate.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "DigitalKyc:Create")]
    [HttpPost("check-similarity")]
    public async Task<ScSoftAtapiResponse> CheckSimilarityRateAsync(CheckSimilarityRateRequest request)
    {
        return await _httpClient.CheckSimilarityRateAsync(request);
    }
}

using LinkPara.ApiGateway.Services.DigitalKyc.HttpClients;
using LinkPara.ApiGateway.Services.DigitalKyc.Models.Requests.Arksigner;
using LinkPara.ApiGateway.Services.DigitalKyc.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Controllers.DigitalKyc;
public class ArksignerController : ApiControllerBase
{
    private readonly IArksignerHttpClient _httpClient;
    public ArksignerController(IArksignerHttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    /// <summary>
    /// Starts the transaction of Arksigner.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "DigitalKyc:Create")]
    [HttpPost("start-kyc")]
    public async Task<ArksignerServiceResponse> StartKycProcessAsync(StartKycProcessRequest request)
    {
        return await _httpClient.StartKycProcessAsync(request);
    }

    /// <summary>
    ///  Checks identity card informations.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "DigitalKyc:Create")]
    [HttpPost("check-id")]
    public async Task<ArksignerServiceResponse> CheckIdentityCardInformationsAsync(CheckIdentityCardInformationsRequest request)
    {
        return await _httpClient.CheckIdentityCardInformationsAsync(request);
    }

    /// <summary>
    ///  Checks nfc informations.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "DigitalKyc:Create")]
    [HttpPost("check-nfc")]
    public async Task<ArksignerServiceResponse> CheckNfcInformationsAsync(CheckNfcInformationsRequest request)
    {
        return await _httpClient.CheckNfcInformationsAsync(request);
    }

    /// <summary>
    ///  Checks face match.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "DigitalKyc:Create")]
    [HttpPost("check-face-match")]
    public async Task<ArksignerServiceResponse> CheckFaceMatchAsync(CheckFaceMatchRequest request)
    {
        return await _httpClient.CheckFaceMatchAsync(request);
    }

    /// <summary>
    ///  Completes the transaction of Arksigner.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "DigitalKyc:Create")]
    [HttpPost("complete-kyc")]
    public async Task<ArksignerServiceResponse> CompleteKycProcessAsync(CompleteKycProcessRequest request)
    {
        return await _httpClient.CompleteKycProcessAsync(request);
    }
}

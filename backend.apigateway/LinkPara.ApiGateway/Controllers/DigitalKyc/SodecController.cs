using LinkPara.ApiGateway.Services.DigitalKyc.HttpClients;
using LinkPara.ApiGateway.Services.DigitalKyc.Models.Requests;
using LinkPara.ApiGateway.Services.DigitalKyc.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Controllers.DigitalKyc;

public class SodecController : ApiControllerBase
{
    private readonly ISodecHttpClient _httpClient;
    public SodecController(ISodecHttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    /// <summary>
    /// Creates the session of sodec kyc.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("create-session")]
    public async Task<SodecCreateSessionResponse> SodecCreateSessionAsync(SodecCreateSessionRequest request)
    {
        return await _httpClient.SodecCreateSessionAsync(request);
    }

    /// <summary>
    /// Completes the session of sodec kyc.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("complete-session")]
    public async Task<SodecCompleteSessionResponse> SodecCompleteSessionAsync(SodecCompleteSessionRequest request)
    {
        return await _httpClient.SodecCompleteSessionAsync(request);
    }
}

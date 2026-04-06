using LinkPara.ApiGateway.Services.DigitalKyc.HttpClients;
using LinkPara.ApiGateway.Services.DigitalKyc.Models.Requests;
using LinkPara.ApiGateway.Services.DigitalKyc.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Controllers.DigitalKyc;

public class DigitalKycController : ApiControllerBase
{
    private readonly IDigitalKycHttpClient _httpClient;

    public DigitalKycController(IDigitalKycHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Starts the session of digital kyc.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("integration-add")]
    public async Task<DigitalKycStartResponse> DigitalKycStartAsync(DigitalKycStartRequest request)
    {
        return await _httpClient.DigitalKycStartAsync(request);
    }

    /// <summary>
    /// Gets the current status the session of digital kyc.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("integration-get")]
    public async Task<IntegrationGetResponse> IntegrationGetAsync(IntegrationGetRequest request)
    {
        return await _httpClient.IntegrationGetAsync(request);
    }

    /// <summary>
    /// Ends the session of digital kyc.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("integration-send")]
    public async Task DigitalKycEndAsync(DigitalKycEndRequest request)
    {
        await _httpClient.DigitalKycEndAsync(request);
    }


    /// <summary>
    /// If kyc state is pending, returns true.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("kyc-state/{userId}")]
    public async Task<bool> GetKycStateByUserIdAsync(string userId)
    {
        return await _httpClient.GetKycStateByUserId(userId);
    }
}
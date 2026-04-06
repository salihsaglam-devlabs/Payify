using LinkPara.Emoney.ApiGateway.Models.Requests;
using LinkPara.Emoney.ApiGateway.Models.Responses;
using LinkPara.Emoney.ApiGateway.Services.HttpClients;
using MassTransit.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Emoney.ApiGateway.Controllers;


public class ProvisionsController : ApiControllerBase
{
    private readonly IProvisionHttpClient _httpClient;

    public ProvisionsController(IProvisionHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Provision from wallet.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [HttpPost("")]
    [Authorize("RequireSignature")]
    public async Task<ProvisionResponse> ProvisionAsync(ProvisionRequest request)
    {
        return  await _httpClient.ProvisionAsync(request);
    }

    /// <summary>
    /// cancel existing provision
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("cancel")]
    [Authorize("RequireSignature")]
    public async Task<ProvisionResponse> CancelProvisionAsync(CancelProvisionRequest request)
    {
        return await _httpClient.CancelProvisionAsync(request);
    }

    /// <summary>
    /// provision status
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("inquire")]
    [Authorize("RequireSignature")]
    public async Task<InquireProvisionResponse> InquireProvisionAsync([FromQuery] InquireProvisionRequest request)
    {
        return await _httpClient.InquireProvisionAsync(request);
    }
}




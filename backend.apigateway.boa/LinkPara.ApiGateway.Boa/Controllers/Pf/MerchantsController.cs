using LinkPara.ApiGateway.Boa.Services.Pf.HttpClients;
using LinkPara.ApiGateway.Boa.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Boa.Services.Pf.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Boa.Controllers.Pf;

public class MerchantsController : ApiControllerBase
{
    private readonly IMerchantHttpClient _merchantHttpClient;

    public MerchantsController(IMerchantHttpClient merchantHttpClient)
    {
        _merchantHttpClient = merchantHttpClient;
    }

    /// <summary>
    /// Creates a new merchant.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("")]
    public async Task<ActionResult<CreateBoaMerchantResponse>> CreateBoaMerchantAsync(CreateBoaMerchantRequest request)
    {
        return await _merchantHttpClient.CreateBoaMerchantAsync(request);
    }
    
    /// <summary>
    /// Returns a merchant
    /// </summary>
    /// <param name="merchantNumber"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("{merchantNumber}")]
    public async Task<ActionResult<BoaMerchantDto>> GetBoaMerchantAsync([FromRoute] string merchantNumber)
    {
        return await _merchantHttpClient.GetBoaMerchantAsync(merchantNumber);
    }
}
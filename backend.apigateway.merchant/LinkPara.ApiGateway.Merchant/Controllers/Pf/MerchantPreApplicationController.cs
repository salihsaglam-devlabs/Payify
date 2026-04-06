using LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Merchant.Controllers.Pf;

public class MerchantPreApplicationController : ApiControllerBase
{
    private readonly IMerchantPreApplicationHttpClient _merchantPreApplicationHttpClient;

    public MerchantPreApplicationController(IMerchantPreApplicationHttpClient merchantPreApplicationHttpClient)
    {
        _merchantPreApplicationHttpClient = merchantPreApplicationHttpClient;
    }
    /// <summary>
    /// Creates a merchant pre application
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("")]
    [AllowAnonymous]
    public async Task<MerchantPreApplicationResponse> CreateMerchantContentAsync(CreateMerchantPreApplicationRequest request)
    {
       return await _merchantPreApplicationHttpClient.CreatePosApplicationAsync(request);
    }
}
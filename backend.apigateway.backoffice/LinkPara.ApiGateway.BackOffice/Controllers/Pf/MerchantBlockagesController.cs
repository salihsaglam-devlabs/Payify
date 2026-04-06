using LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Pf;

public class MerchantBlockagesController : ApiControllerBase
{
    private readonly IMerchantBlockageHttpClient _merchantBlockageHttpClient;

    public MerchantBlockagesController(IMerchantBlockageHttpClient merchantBlockageHttpClient)
    {
        _merchantBlockageHttpClient = merchantBlockageHttpClient;
    }

    /// <summary>
    /// Returns all merchant blockage
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "MerchantBlockage:ReadAll")]
    public async Task<ActionResult<PaginatedList<MerchantBlockageDto>>> GetAllAsync([FromQuery] GetFilterMerchantBlockageRequest request)
    {
        return await _merchantBlockageHttpClient.GetAllAsync(request);
    }

    /// <summary>
    /// Returns merchant blockages
    /// </summary>
    /// <param name="merchantId"></param>
    /// <returns></returns>
    [HttpGet("{merchantId}")]
    [Authorize(Policy = "MerchantBlockage:Read")]
    public async Task<ActionResult<MerchantBlockageDto>> GetByIdAsync([FromRoute] Guid merchantId)
    {
        return await _merchantBlockageHttpClient.GetByMerchantIdAsync(merchantId);
    }

    /// <summary>
    /// Create new blockage for merchant
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("")]
    [Authorize(Policy = "MerchantBlockage:Create")]
    public async Task SaveAsync(SaveMerchantBlockageRequest request)
    {
        await _merchantBlockageHttpClient.SaveAsync(request);
    }

    /// <summary>
    /// Updates a total amount
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("")]
    [Authorize(Policy = "MerchantBlockage:Update")]
    public async Task UpdateAsync(UpdateMerchantBlockageRequest request)
    {
        await _merchantBlockageHttpClient.UpdateAsync(request);
    }

    /// <summary>
    /// Updates a payment date 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("payment-date")]
    [Authorize(Policy = "MerchantBlockage:Update")]
    public async Task UpdateAsync(UpdatePaymentDateRequest request)
    {
        await _merchantBlockageHttpClient.UpdatePaymentDateAsync(request);
    }
}

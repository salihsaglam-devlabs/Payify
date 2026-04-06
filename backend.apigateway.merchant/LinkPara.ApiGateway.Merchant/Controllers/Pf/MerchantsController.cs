using LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Merchant.Controllers.Pf;

public class MerchantsController : ApiControllerBase
{
    private readonly IMerchantHttpClient _merchantHttpClient;

    public MerchantsController(IMerchantHttpClient merchantHttpClient)
    {
        _merchantHttpClient = merchantHttpClient;
    }

    /// <summary>
    /// Returns a merchant informations
    /// </summary>
    /// <returns></returns>
    [HttpGet("summary")]
    [Authorize(Policy = "Merchant:Read")]
    public async Task<ActionResult<MerchantSummaryDto>> GetSummaryByIdAsync()
    {
        return await _merchantHttpClient.GetSummaryByIdAsync();
    }

    /// <summary>
    /// Generate merchant api keys 
    /// </summary>
    /// <param name="merchantId"></param>
    /// <returns></returns>
    [HttpGet("{merchantId}/generate-apiKeys")]
    [Authorize(Policy = "Merchant:Read")]
    public async Task<ActionResult<MerchantApiKeyDto>> GenerateApiKeysAsync([FromRoute] Guid merchantId)
    {
        return await _merchantHttpClient.GenerateApiKeysAsync(merchantId);
    }

    /// <summary>
    /// Updates merchant with put
    /// </summary>
    /// <param name="id"></param>
    /// <param name="merchantPatch"></param>
    /// <returns></returns>
    [HttpPut("update/{id}")]
    [Authorize(Policy = "Merchant:Update")]
    public async Task PutAsync(Guid id, [FromBody] UpdateMerchantPanelDto merchantPatch)
    {
        await _merchantHttpClient.PutAsync(id, merchantPatch);
    }

    /// <summary>
    ///  Updates merchant api keys with patch
    /// </summary>
    /// <param name="merchantId"></param>
    /// <param name="merchantApiKeyPatch"></param>
    /// <returns></returns>
    [HttpPatch("{merchantId}/apiKeys")]
    [Authorize(Policy = "Merchant:Update")]
    public async Task ApiKeyPatchAsync(Guid merchantId, [FromBody] JsonPatchDocument<MerchantApiKeyPatch> merchantApiKeyPatch)
    {
        await _merchantHttpClient.ApiKeyPatchAsync(merchantId, merchantApiKeyPatch);
    }
}

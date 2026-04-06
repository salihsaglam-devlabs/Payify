using LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Merchant.Controllers.Pf;

public class SubMerchantsController : ApiControllerBase
{
    private readonly ISubMerchantHttpClient _subMerchantHttpClient;
    public SubMerchantsController(ISubMerchantHttpClient subMerchantHttpClient)
    {
        _subMerchantHttpClient = subMerchantHttpClient;
    }
    /// <summary>
    /// Returns a sub merchants
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "SubMerchant:ReadAll")]
    public async Task<ActionResult<PaginatedList<SubMerchantDto>>> GetAllAsync([FromQuery] GetAllSubMerchantRequest request)
    {
        return await _subMerchantHttpClient.GetAllAsync(request);
    }

    /// <summary>
    /// Returns a sub merchant
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "SubMerchant:Read")]
    public async Task<ActionResult<SubMerchantDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await _subMerchantHttpClient.GetByIdAsync(id);
    }
    
    /// <summary>
    /// Returns a sub merchants summary.
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "SubMerchant:Read")]
    [HttpGet("summary")]
    public async Task<ActionResult<SubMerchantSummaryDto>> GetSummaryByUserIdAsync()
    {
        return await _subMerchantHttpClient.GetSubMerchantSummary();
    }

    /// <summary>
    /// Create a sub merchant.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "SubMerchant:Create")]
    [HttpPost("")]
    public async Task<Guid> SaveAsync(SaveSubMerchantRequest request)
    {
        return await _subMerchantHttpClient.SaveAsync(request);
    }

    /// <summary>
    /// Updates a sub merchant.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "SubMerchant:Update")]
    [HttpPut("")]
    public async Task UpdateAsync(UpdateSubMerchantRequest request)
    {
        await _subMerchantHttpClient.UpdateAsync(request);
    }

    /// <summary>
    /// Approve/Reject a sub merchant.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "SubMerchant:Update")]
    [HttpPut("approve")]
    public async Task ApproveAsync(ApproveSubMerchantRequest request)
    {
        await _subMerchantHttpClient.ApproveAsync(request);
    }

    /// <summary>
    /// Update a sub merchants.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "SubMerchant:Update")]
    [HttpPut("multiple")]
    public async Task UpdateMultipleAsync(UpdateMultipleSubMerchantRequest request)
    {
        await _subMerchantHttpClient.UpdateMultipleAsync(request);
    }

    /// <summary>
    /// Delete a sub merchant.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "SubMerchant:Delete")]
    [HttpDelete("{id}")]
    public async Task DeleteAsync(Guid id)
    {
        await _subMerchantHttpClient.DeleteAsync(id);
    }
}

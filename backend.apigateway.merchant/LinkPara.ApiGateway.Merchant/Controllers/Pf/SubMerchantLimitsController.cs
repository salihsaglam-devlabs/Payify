using LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Merchant.Controllers.Pf;

public class SubMerchantLimitsController : ApiControllerBase
{
    private readonly ISubMerchantLimitsHttpClient _httpClient;

    public SubMerchantLimitsController(ISubMerchantLimitsHttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    /// <summary>
    /// Returns sub merchant Limit list
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "SubMerchantLimit:ReadAll")]
    public async Task<ActionResult<PaginatedList<SubMerchantLimitDto>>> GetAllAsync([FromQuery] GetAllSubMerchantLimitsRequest request)
    {
        return await _httpClient.GetAllAsync(request);
    }
    
    /// <summary>
    /// Returns a sub merchant Limit
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "SubMerchantLimit:Read")]
    public async Task<ActionResult<SubMerchantLimitDto>> GetById(Guid id)
    {
        return await _httpClient.GetByIdAsync(id);
    }
    
    /// <summary>
    /// Delete sub merchant Limit
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = "SubMerchantLimit:Delete")]
    public async Task DeleteAsync(Guid id)
    { 
        await _httpClient.DeleteAsync(id);
    }
    
    /// <summary>
    /// Saves a sub merchant Limit
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("")]
    [Authorize(Policy = "SubMerchantLimit:Create")]
    public async Task SaveAsync(SaveSubMerchantLimitRequest request)
    { 
        await _httpClient.SaveSubMerchantLimit(request);
    }
    
    /// <summary>
    /// Updates a sub merchant Limit
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("")]
    [Authorize(Policy = "SubMerchantLimit:Update")]
    public async Task UpdateAsync(SubMerchantLimitDto request)
    { 
        await _httpClient.UpdateSubMerchantLimit(request);
    }
}
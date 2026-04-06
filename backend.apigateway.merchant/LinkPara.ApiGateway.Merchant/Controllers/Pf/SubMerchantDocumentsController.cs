using LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Merchant.Controllers.Pf;

public class SubMerchantDocumentsController : ApiControllerBase
{
    private readonly ISubMerchantDocumentsHttpClient _httpClient;

    public SubMerchantDocumentsController(ISubMerchantDocumentsHttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    /// <summary>
    /// Returns sub merchant document list
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "SubMerchantDocument:ReadAll")]
    public async Task<ActionResult<PaginatedList<SubMerchantDocumentDto>>> GetAllAsync([FromQuery] GetAllSubMerchantDocumentRequest request)
    {
        return await _httpClient.GetAllAsync(request);
    }
    
    /// <summary>
    /// Returns a sub merchant document
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "SubMerchantDocument:Read")]
    public async Task<ActionResult<SubMerchantDocumentDto>> GetById(Guid id)
    {
        return await _httpClient.GetByIdAsync(id);
    }
    
    /// <summary>
    /// Delete sub merchant document
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = "SubMerchantDocument:Delete")]
    public async Task DeleteAsync(Guid id)
    { 
        await _httpClient.DeleteAsync(id);
    }
    
    /// <summary>
    /// Saves a sub merchant document
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("")]
    [Authorize(Policy = "SubMerchantDocument:Create")]
    public async Task SaveAsync(SaveSubMerchantDocumentRequest request)
    { 
        await _httpClient.SaveSubMerchantDocument(request);
    }
    
    /// <summary>
    /// Updates a sub merchant document
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("")]
    [Authorize(Policy = "SubMerchantDocument:Update")]
    public async Task UpdateAsync(SubMerchantDocumentDto request)
    { 
        await _httpClient.UpdateSubMerchantDocument(request);
    }
}
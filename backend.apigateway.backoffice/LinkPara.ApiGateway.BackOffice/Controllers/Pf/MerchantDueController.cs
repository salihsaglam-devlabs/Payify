using LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Pf;

public class MerchantDueController : ApiControllerBase
{
    private readonly IMerchantDueHttpClient _merchantDueHttpClient;

    public MerchantDueController(IMerchantDueHttpClient merchantDueHttpClient)
    {
        _merchantDueHttpClient = merchantDueHttpClient;
    }
    
    /// <summary>
    /// Returns a merchant due
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantDue:Read")]
    [HttpGet("{id}")]
    public async Task<MerchantDueDto> GetByIdAsync([FromRoute] Guid id)
    {
        return await _merchantDueHttpClient.GetByIdAsync(id);
    }

    /// <summary>
    /// Returns filtered merchant dues
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantDue:ReadAll")]
    [HttpGet("")]
    public async Task<PaginatedList<MerchantDueDto>> GetAllMerchantDuesAsync([FromQuery] GetAllMerchantDueRequest request)
    {
        return await _merchantDueHttpClient.GetAllMerchantDuesAsync(request);
    }

    /// <summary>
    /// Creates a new merchant due
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantDue:Create")]
    [HttpPost("")]
    public async Task SaveMerchantDueAsync(SaveMerchantDueRequest request)
    {
        await _merchantDueHttpClient.SaveMerchantDueAsync(request);
    }
    
    /// <summary>
    /// Delete merchant due
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantDue:Delete")]
    [HttpDelete("{id}")]
    public async Task DeleteMerchantDueAsync([FromRoute] Guid id)
    {
        await _merchantDueHttpClient.DeleteMerchantDueAsync(id);
    }
}
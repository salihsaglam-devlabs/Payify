using LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Merchant.Controllers.Pf;

public class ParentMerchantsController : ApiControllerBase
{
    private readonly IParentMerchantHttpClient _parentMerchantHttpClient;
    public ParentMerchantsController(IParentMerchantHttpClient parentMerchantHttpClient)
    {
        _parentMerchantHttpClient = parentMerchantHttpClient;
    }

    /// <summary>
    /// Returns filtered parent merchants
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "ParentMerchant:ReadAll")]
    [HttpGet("")]
    public async Task<ActionResult<PaginatedList<MerchantDto>>> GetFilterAsync([FromQuery] GetAllParentMerchantRequest request)
    {
        return await _parentMerchantHttpClient.GetFilterListAsync(request);
    }

    /// <summary>
    /// Returns a parent merchant
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "ParentMerchant:Read")]
    [HttpGet("{id}")]
    public async Task<ActionResult<MerchantDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await _parentMerchantHttpClient.GetByIdAsync(id);
    }
}

using LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Pf;

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
}

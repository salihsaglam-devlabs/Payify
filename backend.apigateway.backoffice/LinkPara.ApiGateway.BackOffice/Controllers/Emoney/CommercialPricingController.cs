using LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Emoney;

public class CommercialPricingController : ApiControllerBase
{
    private readonly ICommercialPricingHttpClient _commercialPricingHttpClient;

    public CommercialPricingController(ICommercialPricingHttpClient commercialPricingHttpClient)
    {
        _commercialPricingHttpClient = commercialPricingHttpClient;
    }
    
    /// <summary>
    /// Returns all commercial pricings
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "CommercialPricing:ReadAll")]
    [HttpGet("all")]
    public async Task<PaginatedList<PricingCommercialDto>> GetAll([FromQuery] CommercialPricingFilterRequest request)
    {
        return await _commercialPricingHttpClient.GetAll(request);
    }
    
    /// <summary>
    /// Adds new Commercial pricing record
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "CommercialPricing:Create")]
    [HttpPost("create")]
    public async Task CreatePricingCommercial(SaveCommercialPricingRequest request)
    {
        await _commercialPricingHttpClient.CreatePricingCommercial(request);
    }
    
    /// <summary>
    /// Updates commercial pricing record
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "CommercialPricing:Update")]
    [HttpPut("update")]
    public async Task UpdatePricingCommercial(UpdateCommercialPricingRequest request)
    {
        await _commercialPricingHttpClient.UpdatePricingCommercial(request);
    }
    
    /// <summary>
    /// Set record's status to passive
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "CommercialPricing:Delete")]
    [HttpDelete("{id}")]
    public async Task DeletePricingCommercial(Guid id)
    {
        await _commercialPricingHttpClient.DeletePricingCommercial(id);
    }  

}
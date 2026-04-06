using LinkPara.ApiGateway.BackOffice.Services.Billing.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Billing;

public class BillingCommissionController : ApiControllerBase
{
    private readonly IBillingCommissionHttpClient _billingCommissionHttpClient;

    public BillingCommissionController(IBillingCommissionHttpClient billingCommissionHttpClient)
    {
        _billingCommissionHttpClient = billingCommissionHttpClient;
    }


    /// <summary>
    /// get billing commission transactions
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "BillingCommission:ReadAll")]
    [HttpGet("")]
    public async Task<PaginatedList<BillingCommissionDto>> GetCommissionsAsync([FromQuery] GetAllBillingCommissionRequest request)
    {
        return await _billingCommissionHttpClient.GetCommissionsAsync(request);
    }
    
    /// <summary>
    /// get billing commission by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "BillingCommission:Read")]
    [HttpGet("{id}")]
    public async Task<BillingCommissionDto> GetCommissionAsync([FromRoute] Guid id)
    {
        return await _billingCommissionHttpClient.GetCommissionAsync(id);
    }
    
    /// <summary>
    /// delete billing commission
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "BillingCommission:Delete")]
    [HttpDelete("{id}")]
    public async Task DeleteCommissionAsync([FromRoute] Guid id)
    {
        await _billingCommissionHttpClient.DeleteCommissionAsync(id);
    }
    
    /// <summary>
    /// create billing commission
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "BillingCommission:Create")]
    [HttpPost("")]
    public async Task CreateCommissionAsync([FromBody] CreateBillingCommissionRequest request)
    {
        await _billingCommissionHttpClient.CreateCommissionAsync(request);
    }
}
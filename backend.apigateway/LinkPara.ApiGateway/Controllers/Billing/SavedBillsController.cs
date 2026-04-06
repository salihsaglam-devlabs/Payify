using LinkPara.ApiGateway.Services.Billing.HttpClients;
using LinkPara.ApiGateway.Services.Billing.Models.Requests;
using LinkPara.ApiGateway.Services.Billing.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Controllers.Billing;

public class SavedBillsController : ApiControllerBase
{
    private readonly ISavedBillHttpClient _savedBillHttpClient;

    public SavedBillsController(ISavedBillHttpClient savedBillHttpClient)
    {
        _savedBillHttpClient = savedBillHttpClient;
    }

    /// <summary>
    /// save new bill
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Billing:Create")]
    [HttpPost("")]
    public async Task SaveAsync([FromBody] CreateSavedBillRequest request)
    {
        await _savedBillHttpClient.SaveAsync(request);
    }

    /// <summary>
    /// get all saved bills
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Billing:ReadAll")]
    [HttpGet("")]
    public async Task<PaginatedList<SavedBillDto>> GetAllAsync([FromQuery] SavedBillFilterRequest request)
    {
        return await _savedBillHttpClient.GetAllAsync(request);
    }

    /// <summary>
    /// update saved bill
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Billing:Update")]
    [HttpPut("")]
    public async Task UpdateAsync([FromBody] UpdateSavedBillRequest request)
    {
        await _savedBillHttpClient.UpdateAsync(request);
    }

    /// <summary>
    /// delete saved bill
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "Billing:Delete")]
    [HttpDelete("{id}")]
    public async Task DeleteAsync([FromRoute] Guid id)
    {
        await _savedBillHttpClient.DeleteAsync(id);
    }
}
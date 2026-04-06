using LinkPara.ApiGateway.BackOffice.Services.Epin.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Epin.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Epin.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Epin;

public class EpinReconciliationsController : ApiControllerBase
{
    private readonly IEpinReconciliationHttpClient _httpClient;

    public EpinReconciliationsController(IEpinReconciliationHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Get ReconciliationSummaries Filter
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "EpinReconciliation:ReadAll")]
    public async Task<PaginatedList<EpinReconciliationSummaryDto>> GetFilterReconciliationSummariesAsync([FromQuery] GetFilterReconciliationSummariesRequest request)
    {
        return await _httpClient.GetFilterReconciliationSummariesAsync(request);
    }

    /// <summary>
    /// Get Reconciliation Summary
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "EpinReconciliation:Read")]
    public async Task<EpinReconciliationSummaryDto> GetReconciliationSummaryAsync([FromRoute] Guid id)
    {
        return await _httpClient.GetReconciliationSummaryAsync(id);
    }

    /// <summary>
    /// Reconciliation By Date
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("")]
    [Authorize(Policy = "EpinReconciliation:Create")]
    public async Task ReconciliationByDateAsync([FromBody] ReconciliationByDateRequest request)
    {
        await _httpClient.ReconciliationByDateAsync(request);
    }
}

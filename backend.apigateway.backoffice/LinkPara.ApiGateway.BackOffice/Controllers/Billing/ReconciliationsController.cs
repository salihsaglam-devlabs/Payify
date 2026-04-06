using LinkPara.ApiGateway.BackOffice.Services.Billing.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Billing;

public class ReconciliationsController : ApiControllerBase
{
    private readonly IReconciliationHttpClient _reconciliationHttpClient;

    public ReconciliationsController(IReconciliationHttpClient reconciliationHttpClient)
    {
        _reconciliationHttpClient = reconciliationHttpClient;
    }

    /// <summary>
    /// get reconciliation summary
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("summary")]
    [Authorize(Policy = "Reconciliation:ReadAll")]
    public async Task<PaginatedList<SummaryDto>> GetSummaryAsync([FromQuery] SummaryFilterRequest request)
    {
        return await _reconciliationHttpClient.GetSummaryAsync(request);
    }

    /// <summary>
    /// get institution reconciliation summary
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("institution-summary")]
    [Authorize(Policy = "Reconciliation:ReadAll")]
    public async Task<PaginatedList<InstitutionSummaryDto>> GetInstitutionSummaryAsync([FromQuery] InstitutionSummaryFilterRequest request)
    {
        return await _reconciliationHttpClient.GetInstitutionSummaryAsync(request);
    }

    /// <summary>
    /// get institution reconciliation detail
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("institution-detail")]
    [Authorize(Policy = "Reconciliation:ReadAll")]
    public async Task<PaginatedList<InstitutionDetailDto>> GetInstitutionDetailAsync([FromQuery] InstitutionDetailFilterRequest request)
    {
        return await _reconciliationHttpClient.GetInstitutionDetailAsync(request);
    }

    /// <summary>
    /// retry reconciliation for an institution
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("retry-institution")]
    [Authorize(Policy = "Reconciliation:ReadAll")]
    public async Task<RetryReconciliationInstitutionResponseDto> RetryInstitutionReconciliation([FromQuery] ReconciliationInstitutionRetryRequest request)
    {
        return await _reconciliationHttpClient.RetryInstitutionReconciliationAsync(request);
    }

    [HttpPost("reconciliation-job")]
    [Authorize(Policy = "Reconciliation:ReadAll")]
    public async Task<ReconciliationJobResponseDto> DoReconciliationJobAsync([FromQuery] ReconciliationJobRequest request)
    {
        return await _reconciliationHttpClient.DoReconciliationJobAsync(request);
    }
}
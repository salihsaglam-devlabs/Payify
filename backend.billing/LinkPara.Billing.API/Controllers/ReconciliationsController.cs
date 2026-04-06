using LinkPara.Billing.Application.Features.Reconciliations;
using LinkPara.Billing.Application.Features.Reconciliations.Commands.ReconcilationSummary;
using LinkPara.Billing.Application.Features.Reconciliations.Commands.ReconcilationDetail;
using LinkPara.Billing.Application.Features.Reconciliations.Commands.ReconciliationInstitutionDetail;
using LinkPara.Billing.Application.Features.Reconciliations.Queries.GetInstitutionSummary;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Mvc;
using LinkPara.Billing.Application.Features.Reconciliations.Queries.GetInstitutionDetail;
using LinkPara.Billing.Application.Features.Reconciliations.Commands.ReconciliationInstitutionRetry;
using LinkPara.Billing.Application.Features.Reconciliations.Queries.GetSummary;
using Microsoft.AspNetCore.Authorization;
using LinkPara.Billing.Application.Features.Reconciliations.Commands.ReconciliationJob;

namespace LinkPara.Billing.API.Controllers;

public class ReconciliationsController : ApiControllerBase
{

    /// <summary>
    /// perform reconciliation summary report
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Reconciliation:ReadAll")]
    [HttpPost("summary")]
    public async Task<ReconciliationSummaryResponseDto> PerformReconciliationSummaryAsync([FromBody] PerformReconcilationSummaryCommand request)
    {
        return await Mediator.Send(request);
    }

    /// <summary>
    /// perform reconciliation details
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Reconciliation:ReadAll")]
    [HttpPost("summary-detail")]
    public async Task<ReconciliationDetailsResponseDto> PerformReconciliationDetailsAsync([FromBody] PerformReconciliationDetailCommand request)
    {
        return await Mediator.Send(request);
    }

    /// <summary>
    /// perform institution reconciliation details
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Reconciliation:ReadAll")]
    [HttpPost("institution-detail")]
    public async Task<InstitutionPaymentDetailResponseDto> PerformReconciliationInstitutionDetailAsync([FromBody] PerformReconciliationInstitutionDetailCommand request)
    {
        return await Mediator.Send(request);
    }

    /// <summary>
    /// get reconciliation summary
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Reconciliation:ReadAll")]
    [HttpGet("summary")]
    public async Task<PaginatedList<SummaryDto>> GetSummaryAsync([FromQuery] GetSummaryQuery request)
    {
        return await Mediator.Send(request);
    }

    /// <summary>
    /// get instutition summary records
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Reconciliation:ReadAll")]
    [HttpGet("institution-summary")]
    public async Task<PaginatedList<InstitutionSummaryDto>> GetInstitutionSummaryAsync([FromQuery] GetInstitutionSummaryQuery request)
    {
        return await Mediator.Send(request);
    }

    /// <summary>
    /// get institution detail records
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Institution:ReadAll")]
    [HttpGet("institution-detail")]
    public async Task<PaginatedList<InstitutionDetailDto>> GetInstitutionDetailAsync([FromQuery] GetInstitutionDetailQuery request)
    {
        return await Mediator.Send(request);
    }

    /// <summary>
    /// retry reconciliation for given institution
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Institution:Read")]
    [HttpPost("retry-institution")]
    public async Task<RetryReconciliationInstitutionResponseDto> RetryInstitutionReconciliationAsync(ReconciliationInstitutionRetryCommand request)
    {
        return await Mediator.Send(request);
    }

    /// <summary>
    /// trigger reconciliation for given vendor and date
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Reconciliation:ReadAll")]
    [HttpPost("reconciliation-job")]
    public async Task<ReconciliationJobResponseDto> DoReconciliationJobAsync(ReconciliationJobCommand request)
    {
        return await Mediator.Send(request);
    }
}
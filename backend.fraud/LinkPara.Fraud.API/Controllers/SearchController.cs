using LinkPara.Fraud.Application.Commons.Models;
using LinkPara.Fraud.Application.Commons.Models.Searchs;
using LinkPara.Fraud.Application.Features.OngoingMonitorings;
using LinkPara.Fraud.Application.Features.OngoingMonitorings.Commands.RemoveOngoingMonitoring;
using LinkPara.Fraud.Application.Features.OngoingMonitorings.Queries.GetAllOngoingMonitorings;
using LinkPara.Fraud.Application.Features.Searchs;
using LinkPara.Fraud.Application.Features.Searchs.SearchByIdentities;
using LinkPara.Fraud.Application.Features.Searchs.SearchByNames;
using LinkPara.Fraud.Application.Features.Transactions.Queries.GetAllSearches;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Fraud.API.Controllers;

public class SearchController : ApiControllerBase
{
    /// <summary>
    /// Indicates whether the name is blacklisted.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<SearchResponse>> GetSearchByNameAsync([FromQuery] SearchByNameQuery request)
    {
        return await Mediator.Send(request);
    }

    /// <summary>
    /// Indicates whether Identity Number is blacklisted.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "FraudSearch:Read")]
    [HttpGet("{id}")]
    public async Task<ActionResult<SearchResponse>> GetSearchByIdentityAsync([FromRoute] string id)
    {
        return await Mediator.Send(new SearchByIdentityQuery { Id = id});
    }

    /// <summary>
    /// Returns search logs
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [HttpGet("logs")]
    [Authorize(Policy = "FraudSearch:ReadAll")]
    public async Task<PaginatedList<SearchLogDto>> GetSearchLogsAsync([FromQuery] GetAllSearchesQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Returns ongoing monitorings
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [HttpGet("ongoingMonitorings")]
    [Authorize(Policy = "FraudSearch:ReadAll")]
    public async Task<PaginatedList<OngoingMonitoringDto>> GetOngoingMonitoringsAsync([FromQuery] GetAllOngoingMonitoringsQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Remove an ongoing monitoring.
    /// </summary>
    /// <param name="referenceNumber"></param>
    /// <returns></returns>
    [HttpPut("removeOngoingMonitoring")]
    [Authorize(Policy = "FraudSearch:Update")]
    public async Task<BaseResponse> RemoveOngoingMonitoringAsync([FromBody] string referenceNumber)
    {
        return await Mediator.Send(new RemoveOngoingMonitoringCommand { ReferenceNumber = referenceNumber});
    }

}

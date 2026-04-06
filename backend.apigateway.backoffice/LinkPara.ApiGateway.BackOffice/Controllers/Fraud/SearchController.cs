using LinkPara.ApiGateway.BackOffice.Commons.Helpers;
using LinkPara.ApiGateway.BackOffice.Commons.Models;
using LinkPara.ApiGateway.BackOffice.Services.Fraud.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Fraud.Models.Request;
using LinkPara.ApiGateway.BackOffice.Services.Fraud.Models.Response;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Fraud
{
    public class SearchController : ApiControllerBase
    {
        private readonly ISearchClient _searchClient;
        public SearchController(ISearchClient searchClient) 
        {
            _searchClient = searchClient;
        }

        /// <summary>
        /// Returns search logs
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("logs")]
        [Authorize(Policy = "FraudSearch:ReadAll")]
        public async Task<ActionResult<PaginatedList<SearchLogResponse>>> GetSearchLogsAsync([FromQuery] GetAllSearchesRequest request)
        {
            return await _searchClient.GetAllAsync(request);
        }

        /// <summary>
        /// Returns ongoing monitorings
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("ongoingMonitorings")]
        [Authorize(Policy = "FraudSearch:ReadAll")]
        public async Task<ActionResult<PaginatedList<OngoingMonitoringResponse>>> GetOngoingMonitoringsAsync([FromQuery] GetAllOngoingMonitoringRequest request)
        {
            return await _searchClient.GetAllOngoingMonitoringAsync(request);
        }

        /// <summary>
        /// Remove an ongoing monitoring.
        /// </summary>
        /// <param name="referenceNumber"></param>
        /// <returns></returns>
        [HttpPut("removeOngoingMonitoring")]
        [Authorize(Policy = "FraudSearch:Update")]
        public async Task<ActionResult<BaseResponse>> RemoveOngoingMonitoringAsync(string referenceNumber)
        {
           return await _searchClient.RemoveOngoingMonitoringAsync(referenceNumber);
        }
    }
}

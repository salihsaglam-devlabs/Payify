using LinkPara.ApiGateway.Services.Fraud.HttpClients;
using LinkPara.ApiGateway.Services.Fraud.Models.Request;
using LinkPara.ApiGateway.Services.Fraud.Models.Response;
using LinkPara.HttpProviders.Fraud.Models;
using LinkPara.SharedModels.Pagination;
using MassTransit.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SearchByNameRequest = LinkPara.ApiGateway.Services.Fraud.Models.Request.SearchByNameRequest;

namespace LinkPara.ApiGateway.Controllers.Fraud
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
        /// Indicates whether the name is blacklisted.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<PaginatedList<SearchLogResponse>>> GetSearchByNameAsync([FromQuery] SearchByNameRequest request)
        {
            return await _searchClient.GetSearchByNameAsync(request);
        }
    }
}

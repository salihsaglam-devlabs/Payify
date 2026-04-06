using LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Pf
{
    public class MerchantHistoryController : ApiControllerBase
    {
        private readonly IMerchantHistoryHttpClient _merchantHistoryHttpClient;

        public MerchantHistoryController(IMerchantHistoryHttpClient merchantHistoryHttpClient)
        {
            _merchantHistoryHttpClient = merchantHistoryHttpClient;
        }

        /// <summary>
        /// Returns filtered merchant histories
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("filter")]
        [Authorize(Policy = "MerchantHistory:ReadAll")]
        public async Task<ActionResult<PaginatedList<MerchantHistoryDto>>> GetFilterAsync(
            [FromQuery] GetFilterMerchantHistoryRequest request)
        {
            return await _merchantHistoryHttpClient.GetFilterListAsync(request);
        }

        [HttpGet("")]
        [Authorize(Policy = "MerchantHistory:ReadAll")]
        public async Task<ActionResult<PaginatedList<MerchantHistoryDto>>> GetAllAsync(
            [FromQuery] GetAllMerchantHistoryRequest request)
        {
            return await _merchantHistoryHttpClient.GetAllParameterAsync(request);
        }
    }
}

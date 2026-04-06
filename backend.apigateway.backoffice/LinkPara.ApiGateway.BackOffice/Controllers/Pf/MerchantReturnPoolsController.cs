using LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
namespace LinkPara.ApiGateway.BackOffice.Controllers.Pf
{
    public class MerchantReturnPoolsController : ApiControllerBase
    {
        private readonly IMerchantReturnPoolHttpClient _httpClient;

        public MerchantReturnPoolsController(IMerchantReturnPoolHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Analyst takes action for merchant return pool
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost("")]
        [Authorize(Policy = "MerchantReturnPool:Update")]
        public async Task<ActionResult<ReturnResponse>> ActionMerchantReturnPoolAsync(ActionMerchantReturnPoolRequest command)
        {
            return await _httpClient.ActionMerchantReturnPoolAsync(command);
        }

        /// <summary>
        /// Gets merchant return pool
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpGet("")]
        [Authorize(Policy = "MerchantReturnPool:ReadAll")]
        public async Task<ActionResult<PaginatedList<MerchantReturnPoolDto>>> GetMerchantReturnPoolAsync([FromQuery] GetMerchantReturnPoolsRequest command)
        {
            return await _httpClient.GetFilterListAsync(command);
        }
    }
}

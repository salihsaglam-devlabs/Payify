using LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Pf
{
    public class MerchantLimitsController : ApiControllerBase
    {
        private readonly IMerchantLimitHttpClient _merchantLimitHttpClient;

        public MerchantLimitsController(IMerchantLimitHttpClient merchantLimitHttpClient)
        {
            _merchantLimitHttpClient = merchantLimitHttpClient;
        }

        /// <summary>
        /// Returns filtered merchant limits
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Policy = "MerchantLimit:ReadAll")]
        [HttpGet("")]
        public async Task<ActionResult<PaginatedList<MerchantLimitDto>>> GetFilterAsync(
            [FromQuery] GetFilterMerchantLimitRequest request)
        {
            return await _merchantLimitHttpClient.GetFilterListAsync(request);
        }

        /// <summary>
        /// Create a merchant limit
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="AlreadyInUseException"></exception>
        [Authorize(Policy = "MerchantLimit:Create")]
        [HttpPost("")]
        public async Task SaveAsync(SaveMerchantLimitRequest request)
        {
            await _merchantLimitHttpClient.SaveAsync(request);
        }

        /// <summary>
        /// Update a merchant limit
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Policy = "MerchantLimit:Update")]
        [HttpPut("")]
        public async Task UpdateAsync(UpdateMerchantLimitRequest request)
        {
            await _merchantLimitHttpClient.UpdateAsync(request);
        }
    }
}

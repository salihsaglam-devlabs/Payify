using LinkPara.PF.Application.Features.MerchantHistory;
using LinkPara.PF.Application.Features.MerchantHistory.Queries.GetAllMerchantHistory;
using LinkPara.PF.Application.Features.Merchants.Queries.GetFilterMerchant;
using LinkPara.PF.Application.Features.Merchants;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Mvc;
using LinkPara.PF.Application.Features.MerchantHistory.Queries.GetFilterMerchantHistory;
using Microsoft.AspNetCore.Authorization;

namespace LinkPara.PF.API.Controllers
{
    public class MerchantHistoryController : ApiControllerBase
    {
        /// <summary>
        /// Returns all merchant history
        /// </summary>
        /// <returns></returns>
        [Authorize(Policy = "MerchantHistory:ReadAll")]
        [HttpGet("")]
        public async Task<ActionResult<PaginatedList<MerchantHistoryDto>>> GetAllAsync([FromQuery] GetAllMerchantHistoryQuery query)
        {
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Returns filtered merchant histories
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [Authorize(Policy = "MerchantHistory:ReadAll")]
        [HttpGet("filter")]
        public async Task<ActionResult<PaginatedList<MerchantHistoryDto>>> GetFilterAsync([FromQuery] GetFilterMerchantHistoryQuery query)
        {
            return await Mediator.Send(query);
        }
    }
}

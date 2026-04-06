using LinkPara.PF.Application.Commons.Models.Merchants;
using LinkPara.PF.Application.Features.MerchantLimits.Command.SaveMerchantLimit;
using LinkPara.PF.Application.Features.MerchantLimits.Command.UpdateMerchantLimit;
using LinkPara.PF.Application.Features.MerchantLimits.Queries.GetFilterMerchantLimits;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.PF.API.Controllers
{
    public class MerchantLimitsController : ApiControllerBase
    {
        /// <summary>
        /// Returns filtered merchant limits
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [Authorize(Policy = "MerchantLimit:ReadAll")]
        [HttpGet("")]
        public async Task<ActionResult<PaginatedList<MerchantLimitDto>>> GetFilterAsync([FromQuery] GetFilterMerchantLimitsQuery query)
        {
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Create a merchant limit
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [Authorize(Policy = "MerchantLimit:Create")]
        [HttpPost("")]
        public async Task SaveAsync(SaveMerchantLimitCommand command)
        {
            await Mediator.Send(command);
        }

        /// <summary>
        /// Update a merchant limit
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [Authorize(Policy = "MerchantLimit:Update")]
        [HttpPut("")]
        public async Task UpdateAsync(UpdateMerchantLimitCommand command)
        {
            await Mediator.Send(command);
        }
    }
}

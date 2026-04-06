using LinkPara.PF.Application.Commons.Models.MerchantDeductions;
using LinkPara.PF.Application.Features.MerchantDeductions.Queries.GetFilterMerchantDeductionQuery;
using LinkPara.PF.Application.Features.MerchantDeductions.Queries.GetMerchantDeductionByIdQuery;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.PF.API.Controllers
{
    public class MerchantDeductionController : ApiControllerBase
    {
        /// <summary>
        /// Returns filtered merchant deductions
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [Authorize(Policy = "MerchantDeduction:ReadAll")]
        [HttpGet("")]
        public async Task<ActionResult<PaginatedList<MerchantDeductionDto>>> GetFilterAsync([FromQuery] GetFilterMerchantDeductionQuery query)
        {
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Returns a merchant deduction
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Policy = "MerchantDeduction:Read")]
        [HttpGet("{id}")]
        public async Task<ActionResult<DeductionDetailsResponse>> GetByIdAsync([FromRoute] Guid id)
        {
            return await Mediator.Send(new GetMerchantDeductionByIdQuery { Id = id });
        }

    }
}

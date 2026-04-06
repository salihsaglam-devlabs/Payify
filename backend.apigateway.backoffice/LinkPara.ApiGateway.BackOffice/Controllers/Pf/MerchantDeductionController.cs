using LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Pf
{
    public class MerchantDeductionController : ApiControllerBase
    {
        private readonly IMerchantDeductionHttpClient _merchantDeductionHttpClient;

        public MerchantDeductionController(IMerchantDeductionHttpClient merchantDeductionHttpClient)
        {
            _merchantDeductionHttpClient = merchantDeductionHttpClient;
        }

        /// <summary>
        /// Returns filtered merchant deductions
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Policy = "MerchantDeduction:ReadAll")]
        [HttpGet("")]
        public async Task<ActionResult<PaginatedList<MerchantDeductionDto>>> GetFilterAsync([FromQuery] GetFilterMerchantDeductionRequest request)
        {
            return await _merchantDeductionHttpClient.GetAllMerchantDeductionsAsync(request);
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
            return await _merchantDeductionHttpClient.GetByIdAsync(id);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using LinkPara.PF.Application.Features.Statements.Queries.GetMerchantStatementDetail;
using LinkPara.PF.Application.Features.MerchantStatements;
using LinkPara.PF.Application.Features.MerchantStatements.Queries.DownloadMerchantStatement;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;

namespace LinkPara.PF.API.Controllers
{
    public class MerchantStatementController : ApiControllerBase
    {
        /// <summary>
        /// Returns filtered Merchant Statements
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [Authorize(Policy = "MerchantStatement:ReadAll")] 
        [HttpGet("")]
        public async Task<ActionResult<PaginatedList<MerchantStatementDto>>> GetMerchantStatementDetailAsync([FromQuery] GetMerchantStatementDetailQuery query)
        {
            return await Mediator.Send(query);
        }
        
        /// <summary>
        /// Returns merchant statement file
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [Authorize(Policy = "MerchantStatement:ReadAll")] 
        [HttpGet("download")]
        public async Task<IActionResult> DownloadMerchantStatementAsync([FromQuery] DownloadMerchantStatementQuery query)
        {
            return await Mediator.Send(query);
        }
    }
}

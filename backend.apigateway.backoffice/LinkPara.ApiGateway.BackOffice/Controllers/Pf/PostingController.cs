using LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Pf
{
    public class PostingController : ApiControllerBase
    {
        private readonly IPostingHttpClient _postingHttpClient;

        public PostingController(IPostingHttpClient postingHttpClient)
        {
            _postingHttpClient = postingHttpClient;
        }

        /// <summary>
        /// Get All Transfer Errors
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("transfer-error")]
        [Authorize(Policy = "PostingError:ReadAll")]
        public async Task<ActionResult<PaginatedList<PostingTransferErrorDto>>> GetAllAsync([FromQuery] GetAllPostingTransferErrorRequest request)
        {
            return await _postingHttpClient.GetAllTransferErrorAsync(request);
        }

        /// <summary>
        /// Get Posting Bills
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("bills")]
        [Authorize(Policy = "Posting:ReadAll")]
        public async Task<ActionResult<PaginatedList<PostingBillDto>>> GetBillsAsync([FromQuery] GetPostingBillRequest request)
        {
            return await _postingHttpClient.GetBillsAsync(request);
        }
    }
}

using LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Merchant.Controllers.Pf
{
    public class LinkController : ApiControllerBase
    {
        private readonly ILinkHttpClient _linkHttpClient;

        public LinkController(ILinkHttpClient linkHttpClient)
        {
           _linkHttpClient = linkHttpClient;

        }
        /// <summary>
        /// Create a Payment link
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Policy = "Link:Create")]
        [HttpPost("")]
        public async Task<LinkResponse> SaveAsync(SaveLinkRequest request)
        {
           return await _linkHttpClient.SaveAsync(request);
        }
        /// <summary>
        /// Get Create link requirements.
        /// </summary>
        /// <param name="merchantId"></param>
        /// <returns></returns>
        [Authorize(Policy = "Link:Read")]
        [HttpGet("requirements/{merchantId}")]
        public async Task<LinkRequirementResponse> GetCreateLinkRequirements(Guid merchantId)
        {
            return await _linkHttpClient.GetCreateLinkRequirements(merchantId);
        }
        /// <summary>
        /// Get all links by filter link, transaction and customer info.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Policy = "Link:ReadAll")]
        [HttpPost("payment-report")]
        public async Task<PaginatedList<LinkDto>> GetAllAsync(GetFilterLinkRequest request)
        {
            return await _linkHttpClient.GetAllAsync(request);
        }
        /// <summary>
        /// Delete a link.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Policy = "Link:Delete")]
        [HttpDelete("{id}")]
        public async Task DeleteAsync(Guid id)
        {
            await _linkHttpClient.DeleteLinkAsync(id);
        }
        /// <summary>
        /// Get link payment detail.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Policy = "LinkPayment:Read")]
        [HttpGet("payment-detail")]
        public async Task<PaginatedList<LinkPaymentDetailResponse>> GetLinkPaymentDetailAsync([FromQuery] GetPaymentDetailRequest request)
        {
            return await _linkHttpClient.GetLinkPaymentDetailAsync(request);
        }

    }
}

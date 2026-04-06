using LinkPara.ApiGateway.OpenBanking.Services.Emoney.Responses;
using LinkPara.ApiGateway.Services.Emoney.HttpClients;
using LinkPara.ApiGateway.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Services.Emoney.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;

namespace LinkPara.ApiGateway.Controllers.Emoney
{
    public class ConsentOperationsController : ApiControllerBase
    {
        private readonly IConsentOperationHttpClient _consentOperationHttpClient;

        public ConsentOperationsController(IConsentOperationHttpClient consentOperationHttpClient)
        {
            _consentOperationHttpClient = consentOperationHttpClient;
        }

        /// <summary>
        /// This method is used to get active consent list of customer.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>it returns active consent list</returns>
        [Authorize(Policy = "ConsentOperation:ReadAll")]
        [HttpGet("active-consent")]
        public async Task<List<ConsentDto>> GetActiveConsentListAsync([FromQuery] GetActiveConsentListRequest request)
        {
            return await _consentOperationHttpClient.GetActiveConsentListAsync(request);
        }

        /// <summary>
        /// This method is used to cancel consent.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>it returns true when the otp is sent successfully.</returns>
        [Authorize(Policy = "ConsentOperation:Delete")]
        [HttpPost("cancel-consent")]
        public async Task<ActionResult<CancelConsentResultDto>> CancelConsentAsync([FromBody] CancelConsentRequest request)
        {
            return await _consentOperationHttpClient.CancelConsentAsync(request);
        }

        /// <summary>
        /// This method used to get waiting approval consent list.
        /// </summary>
        /// <returns>it returns waiting approval consent list.</returns>
        [Authorize(Policy = "ConsentOperation:ReadAll")]
        [HttpGet("waiting-approval-consent")]
        public async Task<ActionResult<GetWaitingApprovalConsentResponse>> GetWaitingApprovalConsentsAsync([FromQuery] GetWaitingApprovalConsentRequest request)
        {
            return await _consentOperationHttpClient.GetWaitingApprovalConsentsAsync(request);
        }

        /// <summary>
        /// This method used to get detail info of the consent.
        /// </summary>
        /// <returns>it returns detail consent information</returns>
        [Authorize(Policy = "ConsentOperation:Read")]
        [HttpGet("consent-detail")]
        public async Task<ActionResult<GetConsentDetailResponse>> GetConsentDetailAsync([FromQuery] GetConsentDetailRequest request)
        {
            return await _consentOperationHttpClient.GetConsentDetailAsync(request);
        }

        /// <summary>
        /// This method is used to send sms otp to the customer.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>it returns true when the otp is sent successfully.</returns>
        [Authorize(Policy = "ConsentOperation:Update")]
        [HttpPost("update-consent")]
        public async Task<ActionResult<UpdateConsentResultDto>> UpdateConsentAsync([FromBody] UpdateConsentRequest request)
        {
            return await _consentOperationHttpClient.UpdateConsentAsync(request);
        }

    }
}
using LinkPara.Emoney.Application.Features.ConsentOperations;
using LinkPara.Emoney.Application.Features.ConsentOperations.Commands.CancelConsent;
using LinkPara.Emoney.Application.Features.ConsentOperations.Commands.UpdateConsent;
using LinkPara.Emoney.Application.Features.ConsentOperations.Queries.GetWaitingApprovalConsents;
using LinkPara.Emoney.Application.Features.ConsentOperations.Queries.GetConsentDetail;
using LinkPara.Emoney.Application.Features.ConsentOperations.Queries.GetActiveConsentList;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace LinkPara.Emoney.API.Controllers
{
    public class ConsentOperationsController : ApiControllerBase
    {

        /// <summary>
        /// This method is used to get active consent list of customer.
        /// </summary>
        /// <param name="query"></param>
        /// <returns>it returns active consent list</returns>
        [Authorize(Policy = "ConsentOperation:ReadAll")]
        [HttpGet("active-consent")]
        public async Task<List<ConsentDto>> GetActiveConsentListAsync([FromQuery] GetActiveConsentListQuery query)
        {
            return await Mediator.Send(query);
        }

        /// <summary>
        /// This method is used to cancel consent.
        /// </summary>
        /// <param name="command"></param>
        /// <returns>it returns true when the otp is sent successfully.</returns>
        [Authorize(Policy = "ConsentOperation:Delete")]
        [HttpPost("cancel-consent")]
        public async Task<ActionResult<CancelConsentResultDto>> CancelConsentAsync([FromBody] CancelConsentCommand command)
        {
            return await Mediator.Send(command);
        }

        /// <summary>
        /// This method used to get waiting approval consent list.
        /// </summary>
        /// <returns>it returns waiting approval consent list.</returns>
        [Authorize(Policy = "ConsentOperation:ReadAll")]
        [HttpGet("waiting-approval-consent")]
        public async Task<ActionResult<GetWaitingApprovalConsentResponse>> GetWaitingApprovalConsentsAsync([FromQuery] GetWaitingApprovalConsentQuery query)
        {
            return await Mediator.Send(query);
        }

        /// <summary>
        /// This method used to get detail info of the consent.
        /// </summary>
        /// <returns>it returns detail consent information</returns>
        [Authorize(Policy = "ConsentOperation:Read")]
        [HttpGet("consent-detail")]
        public async Task<ActionResult<GetConsentDetailResponse>> GetConsentDetailAsync([FromQuery] GetConsentDetailQuery query)
        {
            return await Mediator.Send(query);
        }

        /// <summary>
        /// This method is used to send sms otp to the customer.
        /// </summary>
        /// <param name="command"></param>
        /// <returns>it returns true when the otp is sent successfully.</returns>
        [Authorize(Policy = "ConsentOperation:Update")]
        [HttpPost("update-consent")]
        public async Task<ActionResult<UpdateConsentResultDto>> UpdateConsentAsync([FromBody] UpdateConsentCommand command)
        {
            return await Mediator.Send(command);
        }

    }
}
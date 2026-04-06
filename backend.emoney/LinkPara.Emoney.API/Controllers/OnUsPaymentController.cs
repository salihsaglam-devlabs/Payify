using LinkPara.Emoney.Application.Commons.Models.ProvisionModels;
using LinkPara.Emoney.Application.Features.OnUsPayments;
using LinkPara.Emoney.Application.Features.OnUsPayments.Commands;
using LinkPara.Emoney.Application.Features.OnUsPayments.Queries;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Emoney.API.Controllers
{
    public class OnUsPaymentController : ApiControllerBase
    {
        /// <summary>
        /// Gets and Filters Successful OnUsPayments
        /// </summary>
        /// <param name="query"></param>
        [Authorize(Policy = "OnUsPayment:ReadAll")]
        [HttpGet("")]
        public async Task<PaginatedList<OnUsPaymentRequest>> GetOnUsPaymentsAsync([FromQuery] GetOnUsPaymentQuery query)
        {
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Gets OnUsPayment Request Details
        /// </summary>
        /// <param name="onUsPaymentRequestId"></param>
        [Authorize(Policy = "OnUsPayment:Read")]
        [HttpGet("details")]
        public async Task<OnUsPaymentRequest> GetOnUsPaymentDetailsAsync(Guid onUsPaymentRequestId)
        {
            return await Mediator.Send(new GetOnUsPaymentRequestQuery { OnUsPaymentRequestId = onUsPaymentRequestId });
        }

        /// <summary>
        /// OnUsPayment Initialization
        /// </summary>
        /// <param name="query"></param>
        [Authorize(Policy = "OnUsPayment:Create")]
        [HttpPost("init")]
        public async Task<OnUsPaymentResponse> InitOnUsPaymentAsync([FromBody] InitOnUsPaymentCommand query)
        {
            return await Mediator.Send(query);
        }

        /// <summary>
        /// OnUsPayment status Update
        /// </summary>
        /// <param name="query"></param>
        [Authorize(Policy = "OnUsPayment:Update")]
        [HttpPut("updatestatus")]
        public async Task OnUsPaymentUpdateStatusAsync([FromBody] OnUsPaymentUpdateStatusCommand query)
        {
            await Mediator.Send(query);
        }

        /// <summary>
        /// Approve OnUsPayment 
        /// </summary>
        /// <param name="query"></param>
        [Authorize(Policy = "OnUsPayment:Update")]
        [HttpPut("approve")]
        public async Task<ProvisionPreviewResponse> ApproveOnUsPaymentAsync([FromBody] ApproveOnUsPaymentCommand query)
        {
            return await Mediator.Send(query);
        }

    }
}
using LinkPara.IKS.Application.Commons.Models.IKSModels;
using LinkPara.IKS.Application.Commons.Models.IKSModels.Annulments.Response;
using LinkPara.IKS.Application.Features.Annulments.Command.UpdateAnnulment;
using LinkPara.IKS.Application.Features.Annulments.Queries.GetAnnulmentCodes;
using LinkPara.IKS.Application.Features.Annulments.Queries.GetAnnulments;
using LinkPara.IKS.Application.Features.Annuments.Command.SaveAnnulment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.IKS.API.Controllers
{
    public class AnnulmentsController : ApiControllerBase
    {
        [Authorize(Policy = "MerchantIksAnnulment:Create")]
        [HttpPost("")]
        public async Task<IKSResponse<AnnulmentResponse>> SaveAnnulmentAsync(SaveAnnulmentCommand command)
        {
            return await Mediator.Send(command);
        }

        /// <summary>
        /// Updates a annulment
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [Authorize(Policy = "MerchantIksAnnulment:Update")]
        [HttpPut("")]
        public async Task<IKSResponse<AnnulmentResponse>> UpdateAsync(UpdateAnnulmentCommand command)
        {
            return await Mediator.Send(command);
        }

        /// <summary>
        /// Annulment codes
        /// </summary>
        /// <returns></returns>
        [Authorize(Policy = "MerchantIksAnnulment:Read")]
        [HttpGet("")]
        public async Task<IKSResponse<AnnulmentCodesResponse>> AnnulmentCodesAsync()
        {
            return await Mediator.Send(new GetAnnulmentCodesQuery());
        }

        /// <summary>
        /// Annulments query
        /// </summary>
        /// <returns></returns>
        [Authorize(Policy = "MerchantIksAnnulment:Read")]
        [HttpGet("annulmentsQuery")]
        public async Task<IKSResponse<AnnulmentsQueryResponse>> AnnulmentsQueryAsync([FromQuery] GetAnnulmetsQuery query)
        {
            return await Mediator.Send(query);
        }
    }
}

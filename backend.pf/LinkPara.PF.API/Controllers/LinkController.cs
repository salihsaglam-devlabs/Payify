using LinkPara.PF.Application.Features.Links;
using LinkPara.PF.Application.Features.Links.Command.SaveLink;
using LinkPara.PF.Application.Features.Links.Queries.GetCreateLinkRequirement;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LinkPara.PF.Application.Features.Links.Queries.GetAllLink;
using LinkPara.PF.Application.Features.Links.Command.DeleteLink;

namespace LinkPara.PF.API.Controllers
{
    public class LinkController : ApiControllerBase
    {
        /// <summary>
        /// Creates an payment link.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [Authorize(Policy = "Link:Create")]
        [HttpPost("")]
        public async Task<LinkResponse> SaveAsync(SaveLinkCommand command)
        {
           return await Mediator.Send(command);
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
            return await Mediator.Send(new GetCreateLinkRequirementQuery { MerchantId = merchantId});
        }
        /// <summary>
        /// Get all links by filter link, transaction and customer info.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Policy = "Link:ReadAll")]
        [HttpPost("payment-report")]
        public async Task<PaginatedList<LinkDto>> GetAllAsync(GetAllLinkQuery request)
        {
            return await Mediator.Send(request);
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
            await Mediator.Send(new DeleteLinkCommand { Id = id });
        }
    }
}

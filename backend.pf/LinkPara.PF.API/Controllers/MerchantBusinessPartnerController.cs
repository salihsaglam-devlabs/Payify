using LinkPara.PF.Application.Features.MerchantBusinessPartners;
using LinkPara.PF.Application.Features.MerchantBusinessPartners.Command.SaveMerchantBusinessPartner;
using LinkPara.PF.Application.Features.MerchantBusinessPartners.Command.UpdateMerchantBusinessPartner;
using LinkPara.PF.Application.Features.MerchantBusinessPartners.Queries.GetAllMerchantBusinessPartner;
using LinkPara.PF.Application.Features.MerchantBusinessPartners.Queries.GetMerchantBusinessPartnerById;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.PF.API.Controllers
{
    public class MerchantBusinessPartnerController : ApiControllerBase
    {
        /// <summary>
        /// Returns all merchant business partner
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [Authorize(Policy = "MerchantPartner:ReadAll")]
        [HttpGet("")]
        public async Task<ActionResult<PaginatedList<MerchantBusinessPartnerDto>>> GetAllAsync([FromQuery] GetAllMerchantBusinessPartnerQuery query)
        {
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Returns a merchant business partner
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Policy = "MerchantPartner:Read")]
        [HttpGet("{id}")]
        public async Task<ActionResult<MerchantBusinessPartnerDto>> GetByIdAsync([FromRoute] Guid id)
        {
            return await Mediator.Send(new GetMerchantBusinessPartnerByIdQuery { Id = id });
        }

        /// <summary>
        /// Create a merchant business partner
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [Authorize(Policy = "MerchantPartner:Create")]
        [HttpPost("")]
        public async Task SaveAsync(SaveMerchantBusinessPartnerCommand command)
        {
            await Mediator.Send(command);
        }

        /// <summary>
        /// Update a merchant business partner
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [Authorize(Policy = "MerchantPartner:Update")]
        [HttpPut("")]
        public async Task UpdateAsync(UpdateMerchantBusinessPartnerCommand command)
        {
            await Mediator.Send(command);
        }
    }
}

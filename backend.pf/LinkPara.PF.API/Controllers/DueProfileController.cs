using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LinkPara.PF.Application.Commons.Models.DueProfiles;
using LinkPara.PF.Application.Features.DueProfiles.Queries.GetFilterDueProfile;
using LinkPara.PF.Application.Features.DueProfiles.Queries.GetDueProfileById;
using LinkPara.PF.Application.Commons.Models.Merchants;
using LinkPara.PF.Application.Features.Merchants.Command.UpdateMerchant;
using LinkPara.PF.Application.Features.DueProfiles.Command.UpdateDueProfile;
using LinkPara.PF.Application.Features.MerchantContents.Command.CreateMerchantContent;
using LinkPara.PF.Application.Features.DueProfiles.Command.CreateDueProfile;
using LinkPara.PF.Application.Features.Merchants.Command.DeleteMerchant;
using LinkPara.PF.Application.Features.DueProfiles.Command.DeleteDueProfile;

namespace LinkPara.PF.API.Controllers
{
    public class DueProfileController : ApiControllerBase
    {
        /// <summary>
        /// Returns filtered due profiles
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [Authorize(Policy = "DueProfile:ReadAll")]
        [HttpGet("")]
        public async Task<ActionResult<PaginatedList<DueProfileDto>>> GetFilterAsync([FromQuery] GetFilterDueProfileQuery query)
        {
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Returns a due profile
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Policy = "DueProfile:Read")]
        [HttpGet("{id}")]
        public async Task<ActionResult<DueProfileDto>> GetByIdAsync([FromRoute] Guid id)
        {
            return await Mediator.Send(new GetDueProfileByIdQuery { Id = id });
        }

        /// <summary>
        /// Updates a due profile
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [Authorize(Policy = "DueProfile:Update")]
        [HttpPut("")]
        public async Task UpdateAsync(UpdateDueProfileCommand command)
        {
            await Mediator.Send(command);
        }

        /// <summary>
        /// Create a new due profile
        /// </summary>
        /// <param name="command"></param>
        [Authorize(Policy = "DueProfile:Create")]
        [HttpPost]
        public async Task CreateDueProfileAsync(CreateDueProfileCommand command)
        {
            await Mediator.Send(command);
        }

        /// <summary>
        /// Delete due profile
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Policy = "DueProfile:Delete")]
        [HttpDelete("{id}")]
        public async Task DeleteAsync(Guid id)
        {
            await Mediator.Send(new DeleteDueProfileCommand { Id = id });
        }
    }
}

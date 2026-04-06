using LinkPara.Billing.Application.Features.Institutions;
using LinkPara.Billing.Application.Features.Institutions.Commands;
using LinkPara.Billing.Application.Features.Institutions.Queries;
using LinkPara.Billing.Application.Features.Institutions.Queries.GetById;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Billing.API.Controllers
{
    public class InstitutionsController : ApiControllerBase
    {
        /// <summary>
        /// get all institutions list service
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Policy = "Institution:ReadAll")]
        [HttpGet("")]
        public async Task<PaginatedList<InstitutionDto>> GetAllAsync([FromQuery] GetAllInstitutionQuery request)
        {
            return await Mediator.Send(request);
        }

        /// <summary>
        /// get institution by id
        /// </summary>
        /// <param name="institutionId"></param>
        /// <returns></returns>
        [Authorize(Policy = "Institution:Read")]
        [HttpGet("{institutionId}")]
        public async Task<InstitutionDto> GetByIdAsync([FromRoute] Guid institutionId)
        {
            return await Mediator.Send(new GetByIdQuery { InstitutionId = institutionId });
        }

        /// <summary>
        /// update given institution
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Policy = "Institution:Update")]
        [HttpPut("")]
        public async Task UpdateAsync([FromBody] UpdateInstitutionCommand request)
        {
            await Mediator.Send(request);
        }
    }
}

using LinkPara.Billing.Application.Features.Sectors;
using LinkPara.Billing.Application.Features.Sectors.Queries;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Billing.API.Controllers
{
    public class SectorsController : ApiControllerBase
    {
        /// <summary>
        /// get all sector list service
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Policy = "Sector:ReadAll")]
        [HttpGet("")]
        public async Task<PaginatedList<SectorDto>> GetAllAsync([FromQuery] GetAllSectorQuery request) => await Mediator.Send(request);
    }
}
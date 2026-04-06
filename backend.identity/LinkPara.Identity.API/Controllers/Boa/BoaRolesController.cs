using LinkPara.Identity.Application.Features.Roles.Queries;
using LinkPara.Identity.Application.Features.Roles.Queries.GetRoleById;
using LinkPara.Identity.Application.Features.Roles.Queries.RoleFilter;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Identity.API.Controllers.Boa;

public class BoaRolesController : ApiControllerBase
{
    
    [AllowAnonymous]
    [HttpGet("")]
    public async Task<PaginatedList<RoleDto>> GetBoaRolesAsync([FromQuery] RoleFilterQuery query) 
    {
        return await Mediator.Send(query);
    }
}
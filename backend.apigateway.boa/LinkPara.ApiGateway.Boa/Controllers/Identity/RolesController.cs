using LinkPara.ApiGateway.Boa.Services.Identity.HttpClients;
using LinkPara.ApiGateway.Boa.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.Boa.Services.Identity.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Boa.Controllers.Identity;

public class RolesController : ApiControllerBase
{
    private readonly IRolesHttpClient _rolesHttpClient;

    public RolesController(IRolesHttpClient rolesHttpClient)
    {
        _rolesHttpClient = rolesHttpClient;
    }
    
    /// <summary>
    /// Returns all roles
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("")]
    public async Task<PaginatedList<RoleDto>> GetBoaRolesAsync([FromQuery] RoleFilterQuery query) 
    {
        return await _rolesHttpClient.GetBoaRolesAsync(query);
    }
    
}
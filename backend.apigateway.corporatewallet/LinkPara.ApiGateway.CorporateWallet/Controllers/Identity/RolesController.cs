using LinkPara.ApiGateway.CorporateWallet.Services.Identity.HttpClients;
using LinkPara.ApiGateway.CorporateWallet.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.Identity.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.CorporateWallet.Controllers.Identity;

public class RolesController : ApiControllerBase
{
    private readonly IRoleHttpClient _roleHttpClient;

    public RolesController(IRoleHttpClient roleHttpClient)
    {
        _roleHttpClient = roleHttpClient;
    }

    /// <summary>
    /// Lists all created roles.
    /// </summary>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "Role:ReadAll")]
    public async Task<ActionResult<PaginatedList<RoleDto>>> GetAllAsync([FromQuery] GetRolesRequest request)
    {
        return await _roleHttpClient.GetAllRolesAsync(request);
    }
}
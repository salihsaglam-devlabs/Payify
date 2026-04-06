using LinkPara.ApiGateway.BackOffice.Services.Identity.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Identity;

public class PermissionsController : ApiControllerBase
{
    private readonly IPermissionHttpClient _permissionHttpClient;

    public PermissionsController(IPermissionHttpClient permissionHttpClient)
    {
        _permissionHttpClient = permissionHttpClient;
    }

    /// <summary>
    /// Returns claim list of authorized methods.
    /// </summary>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "Permission:ReadAll")]
    public async Task<ActionResult<List<PermissionDto>>> GetAllAsync()
    {
        return await _permissionHttpClient.GetAllPermissionsAsync();
    }

    /// <summary>
    /// Updates permission some allowed properties.
    /// </summary>
    [HttpPut("{permissionId}")]
    [Authorize(Policy = "Permission:Update")]
    public async Task UpdateAsync([FromRoute] Guid permissionId, UpdatePermissionRequest request)
    {
        await _permissionHttpClient.UpdateAsync(permissionId, request);
    }
}


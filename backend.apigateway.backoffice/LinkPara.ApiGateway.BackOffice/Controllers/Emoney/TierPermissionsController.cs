using LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Emoney;

public class TierPermissionsController : ApiControllerBase
{
    private readonly ITierPermissionHttpClient _tierPermissionHttpClient;

    public TierPermissionsController(ITierPermissionHttpClient tierPermissionHttpClient)
    {
        _tierPermissionHttpClient = tierPermissionHttpClient;
    }
    
    /// <summary>
    /// Returns Tier permissions
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "Limit:ReadAll")]
    [HttpGet("")]
    public async Task<List<TierPermissionResponse>> GetTierPermissionsAsync([FromQuery]GetTierPermissionsRequest request)
    {
        return await _tierPermissionHttpClient.GetTierPermissionsAsync(request);
    }
    
    /// <summary>
    /// Updates tier permission.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("")]
    [Authorize(Policy = "Limit:Update")]
    public async Task PutTierPermissionsAsync(UpdateTierPermissionRequest request)
    {
        await _tierPermissionHttpClient.PutTierPermissionAsync(request);
    }
}
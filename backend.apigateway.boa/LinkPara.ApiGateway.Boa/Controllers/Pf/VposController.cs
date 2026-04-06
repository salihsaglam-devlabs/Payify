using LinkPara.ApiGateway.Boa.Services.Pf.HttpClients;
using LinkPara.ApiGateway.Boa.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Boa.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Boa.Controllers.Pf;

public class VposController : ApiControllerBase
{
    private readonly IVposHttpClient _vposHttpClient;

    public VposController(IVposHttpClient vposHttpClient)
    {
        _vposHttpClient = vposHttpClient;
    }
    
    /// <summary>
    /// Returns filtered Vpos with cost profile
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("")]
    public async Task<ActionResult<PaginatedList<VposDto>>> GetFilterAsync(
        [FromQuery] GetFilterVposRequest request)
    {
        return await _vposHttpClient.GetFilterListAsync(request);
    }
}
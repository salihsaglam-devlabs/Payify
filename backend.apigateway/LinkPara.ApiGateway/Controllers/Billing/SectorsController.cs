using LinkPara.ApiGateway.Services.Billing.HttpClients;
using LinkPara.ApiGateway.Services.Billing.Models.Requests;
using LinkPara.ApiGateway.Services.Billing.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Controllers.Billing;

public class SectorsController : ApiControllerBase
{
    private readonly ISectorHttpClient _sectorHttpClient;

    public SectorsController(ISectorHttpClient sectorHttpClient)
    {
        _sectorHttpClient = sectorHttpClient;
    }

    /// <summary>
    /// get sector list for billing
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns> 
    [Authorize(Policy = "Sector:ReadAll")]
    [HttpGet("")]
    public async Task<ActionResult<PaginatedList<SectorDto>>> GetAllSectorAsync([FromQuery] SectorFilterRequest request)
    {
        return await _sectorHttpClient.GetAllSectorAsync(request);
    }
}
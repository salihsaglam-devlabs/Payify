using LinkPara.ApiGateway.BackOffice.Services.Billing.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Billing;

public class SectorsController : ApiControllerBase
{
    private readonly ISectorHttpClient _sectorHttpClient;

    public SectorsController(ISectorHttpClient sectorHttpClient)
    {
        _sectorHttpClient = sectorHttpClient;
    }

    /// <summary>
    /// get all sectors
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "Sector:ReadAll")]
    public async Task<ActionResult<PaginatedList<SectorDto>>> GetAllSectorAsync([FromQuery] SectorFilterRequest request)
    {
        return await _sectorHttpClient.GetAllSectorAsync(request);
    }
}
using LinkPara.ApiGateway.Services.Epin.HttpClients;
using LinkPara.ApiGateway.Services.Epin.Models.Requests;
using LinkPara.ApiGateway.Services.Epin.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Controllers.Epin;

public class BrandsController : ApiControllerBase
{
    private readonly IEpinHttpClient _epinHttpClient;

    public BrandsController(IEpinHttpClient epinHttpClient)
    {
        _epinHttpClient = epinHttpClient;
    }

    /// <summary>
    /// get all brands list
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "EpinBrand:ReadAll")]
    [HttpGet("")]
    public async Task<ActionResult<PaginatedList<BrandDto>>> GetFilterBrandsAsync([FromQuery] GetFilterBrandsRequest request)
    {
        return await _epinHttpClient.GetFilterBrandsAsync(request);
    }
}

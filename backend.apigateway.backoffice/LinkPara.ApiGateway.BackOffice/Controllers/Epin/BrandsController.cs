using LinkPara.ApiGateway.BackOffice.Services.Epin.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Epin.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Epin.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Epin;

public class BrandsController : ApiControllerBase
{
    private readonly IBrandHttpClient _brandHttpClient;

    public BrandsController(IBrandHttpClient brandHttpClient)
    {
        _brandHttpClient = brandHttpClient;
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
        return await _brandHttpClient.GetFilterBrandsAsync(request);
    }
}

using LinkPara.Epin.Application.Features.Brands;
using LinkPara.Epin.Application.Features.Brands.Queries.GetFilterBrands;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Epin.API.Controllers;

public class BrandsController : ApiControllerBase
{
    /// <summary>
    /// Get Brands List
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "EpinBrand:ReadAll")]
    [HttpGet("")]
    public async Task<PaginatedList<BrandDto>> GetFilterBrandsAsync([FromQuery]GetFilterBrandsQuery request)
    {
        return await Mediator.Send(request);
    }
}

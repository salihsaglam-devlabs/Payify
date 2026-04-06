using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Mvc;
using LinkPara.Epin.Application.Features.Products;
using LinkPara.Epin.Application.Features.Products.Queries.GetFilterProducts;
using Microsoft.AspNetCore.Authorization;

namespace LinkPara.Epin.API.Controllers;

public class ProductsController : ApiControllerBase
{
    /// <summary>
    /// Get Products List
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "EpinProduct:ReadAll")]
    [HttpGet("")]
    public async Task<List<ProductDto>> GetFilterProductsAsync([FromQuery] GetFilterProductsQuery request)
    {
        return await Mediator.Send(request);
    }
}

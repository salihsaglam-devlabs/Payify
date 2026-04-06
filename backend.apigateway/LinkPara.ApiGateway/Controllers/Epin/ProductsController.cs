using LinkPara.ApiGateway.Services.Epin.HttpClients;
using LinkPara.ApiGateway.Services.Epin.Models.Requests;
using LinkPara.ApiGateway.Services.Epin.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Controllers.Epin;

public class ProductsController : ApiControllerBase
{
    private readonly IEpinHttpClient _epinHttpClient;

    public ProductsController(IEpinHttpClient epinHttpClient)
    {
        _epinHttpClient = epinHttpClient;
    }

    /// <summary>
    /// get all product list
    /// </summary>
    /// <param name="publisherId"></param>
    /// <param name="brandId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "EpinProduct:ReadAll")]
    [HttpGet("{publisherId}/{brandId}")]
    public async Task<ActionResult<List<ProductDto>>> GetFilterProductsAsync([FromRoute] Guid publisherId, [FromRoute] Guid brandId, [FromQuery] GetFilterProductsRequest request)
    {
        request.BrandId = brandId;
        request.PublisherId = publisherId;
        return await _epinHttpClient.GetFilterProductsAsync(request);
    }
}

using LinkPara.ApiGateway.BackOffice.Services.Epin.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Epin.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Epin.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Epin;

public class ProductsController : ApiControllerBase
{
    private readonly IProductHttpClient _httpClient;

    public ProductsController(IProductHttpClient httpClient)
    {
        _httpClient = httpClient;
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
        return await _httpClient.GetFilterProductsAsync(request);
    }
}

using LinkPara.ApiGateway.Services.Epin.Models.Requests;
using LinkPara.ApiGateway.Services.Epin.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Services.Epin.HttpClients;

public interface IEpinHttpClient
{
    Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request, Guid userId);
    Task<ActionResult<PaginatedList<BrandDto>>> GetFilterBrandsAsync(GetFilterBrandsRequest request);
    Task<ActionResult<List<ProductDto>>> GetFilterProductsAsync(GetFilterProductsRequest request);
    Task<ActionResult<PaginatedList<PublisherDto>>> GetFilterPublishersAsync(GetFilterPublishersRequest request);
    Task<PaginatedList<UserOrderDto>> GetUserOrdersFilterAsync(GetUserOrdersFilterRequest request);
}

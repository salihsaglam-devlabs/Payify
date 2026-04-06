using LinkPara.ApiGateway.Services.Epin.HttpClients;
using LinkPara.ApiGateway.Services.Epin.Models.Requests;
using LinkPara.ApiGateway.Services.Epin.Models.Responses;
using LinkPara.SharedModels.Pagination;
using MassTransit.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Controllers.Epin;

public class OrdersController : ApiControllerBase
{
    private readonly IEpinHttpClient _epinHttpClient;

    public OrdersController(IEpinHttpClient epinHttpClient)
    {
        _epinHttpClient = epinHttpClient;
    }

    /// <summary>
    /// Create Order
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("")]
    [Authorize(Policy = "EpinOrder:Create")]
    public async Task<CreateOrderResponse> CreateOrderAsync([FromBody] CreateOrderRequest request)
    {
        return await _epinHttpClient.CreateOrderAsync(request, Guid.Parse(UserId));
    }

    /// <summary>
    /// Get Orders me
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("me")]
    [Authorize(Policy = "EpinOrder:ReadAll")]
    public async Task<PaginatedList<UserOrderDto>> GetUserOrdersFilterAsync([FromQuery] GetUserOrdersFilterRequest request)
    {
        return await _epinHttpClient.GetUserOrdersFilterAsync(request);
    }
}

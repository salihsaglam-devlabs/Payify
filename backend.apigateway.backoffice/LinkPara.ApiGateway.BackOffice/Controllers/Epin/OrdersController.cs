using LinkPara.ApiGateway.BackOffice.Services.Epin.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Epin.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Epin.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Epin;

public class OrdersController : ApiControllerBase
{
    private readonly IOrderHttpClient _httpClient;

    public OrdersController(IOrderHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Get Order Summary
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("summary/{id}")]
    [Authorize(Policy = "EpinOrder:Read")]
    public async Task<OrderSummaryDto> GetOrderSummaryAsync([FromRoute] Guid id)
    {
        return await _httpClient.GetOrderSummaryAsync(id);
    }

    /// <summary>
    /// Get Orders Filter
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "EpinOrder:ReadAll")]
    public async Task<PaginatedList<OrderDto>> GetOrdersFilterAsync([FromQuery] GetOrdersFilterRequest request)
    {
        return await _httpClient.GetOrdersFilterAsync(request);
    }

    /// <summary>
    /// Cancel Order
    /// </summary>
    /// <param name="orderId"></param>
    /// <returns></returns>
    [HttpDelete("cancel/{orderId}")]
    [Authorize(Policy = "EpinOrder:Delete")]
    public async Task CancelOrderAsync([FromRoute] Guid orderId)
    {
        await _httpClient.CancelOrderAsync(orderId);
    }
}

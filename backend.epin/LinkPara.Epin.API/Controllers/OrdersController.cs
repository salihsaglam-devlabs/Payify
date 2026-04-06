using Microsoft.AspNetCore.Mvc;
using LinkPara.Epin.Application.Features.Orders.Commands.CreateOrder;
using LinkPara.Epin.Application.Features.Orders;
using LinkPara.SharedModels.Pagination;
using LinkPara.Epin.Application.Features.Orders.Queries.GetUserOrdersFilter;
using LinkPara.Epin.Application.Features.Orders.Queries.GetOrdersFilter;
using LinkPara.Epin.Application.Features.Orders.Queries.GetOrderSummary;
using LinkPara.Epin.Application.Features.Orders.Commands.CancelOrder;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace LinkPara.Epin.API.Controllers;

public class OrdersController : ApiControllerBase
{
    /// <summary>
    /// Create Order
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "EpinOrder:Create")]
    [HttpPost("")]
    public async Task<CreateOrderResponse> CreateOrderAsync([FromBody] CreateOrderCommand request)
    {
        return await Mediator.Send(request);
    }

    /// <summary>
    /// Cancel Order
    /// </summary>
    /// <param name="orderId"></param>
    /// <returns></returns>
    [Authorize(Policy = "EpinOrder:Delete")]
    [HttpDelete("cancel/{orderId}")]
    public async Task<Unit> CancelOrderAsync([FromRoute] Guid orderId)
    {
        return await Mediator.Send(new CancelOrderCommand { OrderId = orderId });
    }

    /// <summary>
    /// Get User Orders
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "EpinOrder:ReadAll")]
    [HttpGet("{userId}")]
    public async Task<PaginatedList<UserOrderDto>> GetFilterUserOrdersAsync([FromRoute] Guid userId, [FromQuery] GetUserOrdersFilterQuery request)
    {
        request.UserId = userId;
        return await Mediator.Send(request);
    }


    /// <summary>
    /// Get Orders Filter
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "EpinOrder:ReadAll")]
    [HttpGet("")]
    public async Task<PaginatedList<OrderDto>> GetOrdersFilterAsync([FromQuery] GetOrdersFilterQuery request)
    {
        return await Mediator.Send(request);
    }

    /// <summary>
    /// Get Orders Filter
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "EpinOrder:Read")]
    [HttpGet("summary/{id}")]
    public async Task<OrderSummaryDto> GetOrderSummaryAsync([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetOrderSummaryQuery { OrderId = id });
    }
}

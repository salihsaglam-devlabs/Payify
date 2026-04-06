using LinkPara.Epin.Application.Features.Orders;
using LinkPara.Epin.Application.Features.Orders.Commands.CancelOrder;
using LinkPara.Epin.Application.Features.Orders.Commands.CreateOrder;
using LinkPara.Epin.Application.Features.Orders.Queries.GetOrdersFilter;
using LinkPara.Epin.Application.Features.Orders.Queries.GetOrderSummary;
using LinkPara.Epin.Application.Features.Orders.Queries.GetUserOrdersFilter;
using LinkPara.Epin.Domain.Entities;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.Epin.Application.Commons.Interfaces;

public interface IOrderService
{
    Task<Unit> CancelOrderAsync(CancelOrderCommand request);
    Task<CreateOrderResponse> CreateOrderAsync(CreateOrderCommand request);
    Task<List<Order>> GetOrdersByDateAsync(DateTime transactionDate);
    Task<PaginatedList<OrderDto>> GetOrdersFilterAsync(GetOrdersFilterQuery request);
    Task<OrderSummaryDto> GetOrderSummaryAsync(GetOrderSummaryQuery request);
    Task<PaginatedList<UserOrderDto>> GetUserOrdersFilterAsync(GetUserOrdersFilterQuery request);
    Task UpdateReconciliationOrdersAsync(List<Order> orders);
    
}

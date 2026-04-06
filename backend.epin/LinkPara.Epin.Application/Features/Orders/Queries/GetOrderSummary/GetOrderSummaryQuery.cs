using LinkPara.Epin.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Epin.Application.Features.Orders.Queries.GetOrderSummary;

public class GetOrderSummaryQuery : IRequest<OrderSummaryDto>
{
    public Guid OrderId { get; set; }
}

public class GetOrderSummaryQueryHandler : IRequestHandler<GetOrderSummaryQuery, OrderSummaryDto>
{
    private readonly IOrderService _orderService;

    public GetOrderSummaryQueryHandler(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public async Task<OrderSummaryDto> Handle(GetOrderSummaryQuery request, CancellationToken cancellationToken)
    {
        return await _orderService.GetOrderSummaryAsync(request);
    }
}

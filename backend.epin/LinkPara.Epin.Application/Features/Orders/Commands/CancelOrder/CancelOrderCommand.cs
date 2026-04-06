using LinkPara.Epin.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Epin.Application.Features.Orders.Commands.CancelOrder;

public class CancelOrderCommand : IRequest
{
    public Guid OrderId { get; set; }
}


public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand>
{
    private readonly IOrderService _orderService;

    public CancelOrderCommandHandler(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public async Task<Unit> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        return await _orderService.CancelOrderAsync(request);
    }
}
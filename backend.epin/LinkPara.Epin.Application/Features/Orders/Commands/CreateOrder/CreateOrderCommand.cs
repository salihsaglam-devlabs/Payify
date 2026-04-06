using LinkPara.ContextProvider;
using LinkPara.Epin.Application.Commons.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinkPara.Epin.Application.Features.Orders.Commands.CreateOrder;

public class CreateOrderCommand : IRequest<CreateOrderResponse>
{
    public int ProductId { get; set; }
    public decimal Amount { get; set; }
    public string WalletNumber { get; set; }
    public string CurrencyCode { get; set; }
    public Guid PublisherId { get; set; }
    public Guid BrandId { get; set; }
}

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, CreateOrderResponse>
{
    private readonly IOrderService _orderService;
    private readonly IContextProvider _contextProvider;

    private readonly IEmoneyService _eMoneyService;
    private readonly ILogger<CreateOrderCommandHandler> _logger;
    public CreateOrderCommandHandler(IOrderService orderService, IContextProvider contextProvider, IEmoneyService eMoneyService, ILogger<CreateOrderCommandHandler> logger)
    {
        _orderService = orderService;
        _contextProvider = contextProvider;
        _eMoneyService = eMoneyService;
        _logger = logger;
    }

    public async Task<CreateOrderResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        return await _orderService.CreateOrderAsync(request);
    }
}
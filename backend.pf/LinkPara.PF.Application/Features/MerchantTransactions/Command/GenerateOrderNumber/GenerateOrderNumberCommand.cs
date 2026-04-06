using LinkPara.PF.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.PF.Application.Features.MerchantTransactions.Command.GenerateOrderNumber;

public class GenerateOrderNumberCommand : IRequest<OrderNumberResponse>
{
    public Guid MerchantId { get; set; }
    public string OrderNumber { get; set; }
    public bool IsLinkOrderId { get; set; }
}

public class GenerateOrderNumberCommandHandler : IRequestHandler<GenerateOrderNumberCommand, OrderNumberResponse>
{
    private readonly IOrderNumberGeneratorService _orderNumberGeneratorService;
    public GenerateOrderNumberCommandHandler(IOrderNumberGeneratorService orderNumberGeneratorService)
    {
        _orderNumberGeneratorService = orderNumberGeneratorService;
    }
    public async Task<OrderNumberResponse> Handle(GenerateOrderNumberCommand request, CancellationToken cancellationToken)
    {
        return await _orderNumberGeneratorService.GenerateAsync(request);
    }
}

using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.AccountServiceProviders.Commands.CreatePaymentOrder;
public class CreatePaymentOrderCommand : IRequest<PaymentContractDto>
{
    public PaymentContractDto Contract { get; set; }
}

public class CreatePaymentOrderCommandHandler : IRequestHandler<CreatePaymentOrderCommand, PaymentContractDto>
{
    private readonly IOpenBankingService _openBankingService;

    public CreatePaymentOrderCommandHandler(IOpenBankingService openBankingService)
    {
        _openBankingService = openBankingService;
    }

    public async Task<PaymentContractDto> Handle(CreatePaymentOrderCommand request,
        CancellationToken cancellationToken)
    {
        return await _openBankingService.CreatePaymentOrderAsync(request, cancellationToken);
    }
}

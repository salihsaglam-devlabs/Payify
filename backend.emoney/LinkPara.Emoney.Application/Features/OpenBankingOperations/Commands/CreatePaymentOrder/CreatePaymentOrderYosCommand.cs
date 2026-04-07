using LinkPara.ApiGateway.Services.Emoney.Models.Enums;
using LinkPara.Emoney.Application.Commons.Enums;
using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.OpenBankingOperations.Commands.CreatePaymentOrder;
public class CreatePaymentOrderYosCommand : IRequest<PaymentOrderDetailResultDto>
{
    public string ConsentId { get; set; }
    public int AppUserId { get; set; }
    public string HhsCode { get; set; }
}

public class CreatePaymentOrderYosCommandHandler : IRequestHandler<CreatePaymentOrderYosCommand, PaymentOrderDetailResultDto>
{
    private readonly IOpenBankingOperationsService _openBankingOperationsService;

    public CreatePaymentOrderYosCommandHandler(
         IOpenBankingOperationsService openBankingOperationsService)
    {
        _openBankingOperationsService = openBankingOperationsService;
    }

    public async Task<PaymentOrderDetailResultDto> Handle(CreatePaymentOrderYosCommand request,
        CancellationToken cancellationToken)
    {
        return await _openBankingOperationsService.CreatePaymentOrderAsync(request);
    }
}

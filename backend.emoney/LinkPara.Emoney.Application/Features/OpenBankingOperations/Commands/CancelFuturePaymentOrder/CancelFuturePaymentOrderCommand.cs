using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.OpenBankingOperations.Commands.CancelFuturePaymentOrder;
public class CancelFuturePaymentOrderCommand : IRequest<CancelFuturePaymentOrderResultDto>
{
    public string ConsentId { get; set; }    
    public string HhsCode { get; set; }
    public string ApplicationUser { get; set; }    
}


public class CancelFuturePaymentOrderCommandHandler : IRequestHandler<CancelFuturePaymentOrderCommand, CancelFuturePaymentOrderResultDto>
{
    private readonly IOpenBankingOperationsService _openBankingOperationsService;

    public CancelFuturePaymentOrderCommandHandler(
         IOpenBankingOperationsService openBankingOperationsService)
    {
        _openBankingOperationsService = openBankingOperationsService;
    }

    public async Task<CancelFuturePaymentOrderResultDto> Handle(CancelFuturePaymentOrderCommand command,
        CancellationToken cancellationToken)
    {
        return await _openBankingOperationsService.CancelFuturePaymentOrderAsync(command);
    }
}

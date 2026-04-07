using LinkPara.ApiGateway.Services.Emoney.Models.Enums;
using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.PaymentOrderDetail;
public class PaymentOrderDetailQuery : IRequest<PaymentOrderDetailResultDto>
{
    public string HhsCode { get; set; }
    public int AppUserId { get; set; }
    public string ConsentId { get; set; }
}

public class PaymentOrderDetailQueryHandler : IRequestHandler<PaymentOrderDetailQuery, PaymentOrderDetailResultDto>
{
    private readonly IOpenBankingOperationsService _openBankingOperationsService;

    public PaymentOrderDetailQueryHandler(
         IOpenBankingOperationsService openBankingOperationsService)
    {
        _openBankingOperationsService = openBankingOperationsService;
    }

    public async Task<PaymentOrderDetailResultDto> Handle(PaymentOrderDetailQuery request,
        CancellationToken cancellationToken)
    {
        return await _openBankingOperationsService.PaymentOrderDetailQueryAsync(request);
    }
}

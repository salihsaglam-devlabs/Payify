using LinkPara.ApiGateway.Services.Emoney.Models.Enums;
using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetPaymentOrderConsentDetail;
public class GetPaymentOrderConsentDetailQuery : IRequest<PaymentOrderConsentDetailDto>
{
    public string ConsentId { get; set; }
    public int AppUserId { get; set; }
    public string HhsCode { get; set; }
}

public class GetPaymentOrderConsentDetailQueryHandler : IRequestHandler<GetPaymentOrderConsentDetailQuery, PaymentOrderConsentDetailDto>
{
    private readonly IOpenBankingOperationsService _openBankingOperationsService;

    public GetPaymentOrderConsentDetailQueryHandler(
         IOpenBankingOperationsService openBankingOperationsService)
    {
        _openBankingOperationsService = openBankingOperationsService;
    }

    public async Task<PaymentOrderConsentDetailDto> Handle(GetPaymentOrderConsentDetailQuery request,
        CancellationToken cancellationToken)
    {
        return await _openBankingOperationsService.GetPaymentOrderConsentDetailAsync(request);
    }
}

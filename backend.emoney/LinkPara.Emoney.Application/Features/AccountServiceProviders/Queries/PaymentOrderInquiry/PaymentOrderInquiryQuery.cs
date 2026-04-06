using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.AccountServiceProviders.Queries.PaymentOrderInquiry;
public class PaymentOrderInquiryQuery : IRequest<PaymentContractDto>
{
    public string PaymentGuid { get; set; }
}

public class PaymentOrderInquiryQueryHandler : IRequestHandler<PaymentOrderInquiryQuery, PaymentContractDto>
{
    private readonly IOpenBankingService _openBankingService;

    public PaymentOrderInquiryQueryHandler(IOpenBankingService openBankingService)
    {
        _openBankingService = openBankingService;
    }

    public async Task<PaymentContractDto> Handle(PaymentOrderInquiryQuery request,
        CancellationToken cancellationToken)
    {
        return await _openBankingService.PaymentOrderInquiryAsync(request);
    }
}

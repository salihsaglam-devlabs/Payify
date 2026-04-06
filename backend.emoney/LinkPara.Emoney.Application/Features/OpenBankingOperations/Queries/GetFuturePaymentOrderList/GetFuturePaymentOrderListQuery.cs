using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetCardDetail;
public class GetFuturePaymentOrderListQuery : IRequest<GetFuturePaymentOrderListResultDto>
{    
    public string ApplicationUser { get; set; }
    public DateTime BeginDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; }
}

public class GetFuturePaymentOrderListQueryHandler : IRequestHandler<GetFuturePaymentOrderListQuery, GetFuturePaymentOrderListResultDto>
{
    private readonly IOpenBankingOperationsService _openBankingOperationsService;

    public GetFuturePaymentOrderListQueryHandler(
         IOpenBankingOperationsService openBankingOperationsService)
    {
        _openBankingOperationsService = openBankingOperationsService;
    }

    public async Task<GetFuturePaymentOrderListResultDto> Handle(GetFuturePaymentOrderListQuery request,
        CancellationToken cancellationToken)
    {
        return await _openBankingOperationsService.GetFuturePaymentOrderListAsync(request);
    }
}

using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetConsentedAccountBalanceList;
public class GetConsentedAccountBalanceListQuery : IRequest<ConsentedAccountBalancesResultDto>
{
    public int AppUserId { get; set; }
    public string HhsCode { get; set; }
}

public class GetConsentedAccountBalanceListQueryHandler : IRequestHandler<GetConsentedAccountBalanceListQuery, ConsentedAccountBalancesResultDto>
{
    private readonly IOpenBankingOperationsService _openBankingOperationsService;

    public GetConsentedAccountBalanceListQueryHandler(
         IOpenBankingOperationsService openBankingOperationsService)
    {
        _openBankingOperationsService = openBankingOperationsService;
    }

    public async Task<ConsentedAccountBalancesResultDto> Handle(GetConsentedAccountBalanceListQuery request,
        CancellationToken cancellationToken)
    {
        return await _openBankingOperationsService.GetConsentedAccountBalanceListAsync(request);
    }
}

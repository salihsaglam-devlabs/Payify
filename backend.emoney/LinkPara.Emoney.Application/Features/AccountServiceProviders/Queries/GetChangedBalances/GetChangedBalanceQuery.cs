using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.AccountServiceProviders.Queries.GetChangedBalance;
public class GetChangedBalanceQuery : IRequest<List<ChangedBalanceDto>>
{

}

public class GetChangedBalanceQueryHandler : IRequestHandler<GetChangedBalanceQuery, List<ChangedBalanceDto>>
{
    private readonly IOpenBankingService _openBankingService;


    public GetChangedBalanceQueryHandler(
        IOpenBankingService openBankingService)
    {
        _openBankingService = openBankingService;
    }

    public async Task<List<ChangedBalanceDto>> Handle(GetChangedBalanceQuery request,
        CancellationToken cancellationToken)
    {
        return await _openBankingService.GetChangedBalanceAsync(request);

    }
}

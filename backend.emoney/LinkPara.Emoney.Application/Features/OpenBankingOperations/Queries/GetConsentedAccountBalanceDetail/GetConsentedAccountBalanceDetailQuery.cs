using LinkPara.ApiGateway.Services.Emoney.Models.Enums;
using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetConsentedAccountBalanceDetail;
public class GetConsentedAccountBalanceDetailQuery : IRequest<ConsentedAccountBalanceDetailResultDto>
{
    public string AccountReference { get; set; }
    public int AppUserId { get; set; }
    public string HhsCode { get; set; }
}

public class GetConsentedAccountBalanceDetailQueryHandler : IRequestHandler<GetConsentedAccountBalanceDetailQuery, ConsentedAccountBalanceDetailResultDto>
{
    private readonly IOpenBankingOperationsService _openBankingOperationsService;

    public GetConsentedAccountBalanceDetailQueryHandler(
         IOpenBankingOperationsService openBankingOperationsService)
    {
        _openBankingOperationsService = openBankingOperationsService;
    }

    public async Task<ConsentedAccountBalanceDetailResultDto> Handle(GetConsentedAccountBalanceDetailQuery request,
        CancellationToken cancellationToken)
    {
        return await _openBankingOperationsService.GetConsentedAccountBalanceDetailAsync(request);
    }
}

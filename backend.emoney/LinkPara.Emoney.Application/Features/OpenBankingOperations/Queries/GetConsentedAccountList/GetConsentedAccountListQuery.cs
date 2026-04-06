using LinkPara.ApiGateway.Services.Emoney.Models.Enums;
using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetConsentedAccountList;
public class GetConsentedAccountListQuery : IRequest<ConsentedAccountsResultDto>
{
    public string AppUserId { get; set; }
    public string HhsCode { get; set; }
}

public class GetConsentedAccountListQueryHandler : IRequestHandler<GetConsentedAccountListQuery, ConsentedAccountsResultDto>
{
    private readonly IOpenBankingOperationsService _openBankingOperationsService;

    public GetConsentedAccountListQueryHandler (
         IOpenBankingOperationsService openBankingOperationsService)
    {
        _openBankingOperationsService = openBankingOperationsService;
    }

    public async Task<ConsentedAccountsResultDto> Handle(GetConsentedAccountListQuery request,
        CancellationToken cancellationToken)
    {
        return await _openBankingOperationsService.GetConsentedAccountListAsync(request);
    }
}

using LinkPara.ApiGateway.Services.Emoney.Models.Enums;
using LinkPara.Emoney.Application.Commons.Enums;
using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetAccountConsent;
public class GetAccountConsentQuery : IRequest<AccountConsentDetailResultDto>
{
    public string ConsentId { get; set; }
}

public class GetAccountConsentQueryHandler : IRequestHandler<GetAccountConsentQuery, AccountConsentDetailResultDto>
{
    private readonly IOpenBankingOperationsService _openBankingOperationsService;

    public GetAccountConsentQueryHandler(
         IOpenBankingOperationsService openBankingOperationsService)
    {
        _openBankingOperationsService = openBankingOperationsService;
    }

    public async Task<AccountConsentDetailResultDto> Handle(GetAccountConsentQuery request,
        CancellationToken cancellationToken)
    {
        return await _openBankingOperationsService.GetAccountConsentDetailAsync(request);
    }
}

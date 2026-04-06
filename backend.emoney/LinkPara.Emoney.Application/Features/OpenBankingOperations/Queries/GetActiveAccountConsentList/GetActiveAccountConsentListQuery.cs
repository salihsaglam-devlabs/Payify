using LinkPara.ApiGateway.Services.Emoney.Models.Enums;
using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetActiveAccountConsentList;
public class GetActiveAccountConsentListQuery : IRequest<ActiveAccountConsentResultDto>
{
    public string AppUserId { get; set; }
}

public class GetActiveAccountConsentListQueryHandler : IRequestHandler<GetActiveAccountConsentListQuery, ActiveAccountConsentResultDto>
{
    private readonly IOpenBankingOperationsService _openBankingOperationsService;

    public GetActiveAccountConsentListQueryHandler(
         IOpenBankingOperationsService openBankingOperationsService)
    {
        _openBankingOperationsService = openBankingOperationsService;
    }

    public async Task<ActiveAccountConsentResultDto> Handle(GetActiveAccountConsentListQuery request,
        CancellationToken cancellationToken)
    {
        return await _openBankingOperationsService.GetActiveAccountConsentListAsync(request);
    }
}

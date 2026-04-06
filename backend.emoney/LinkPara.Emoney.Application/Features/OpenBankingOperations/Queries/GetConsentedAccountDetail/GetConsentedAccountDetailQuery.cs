using LinkPara.ApiGateway.Services.Emoney.Models.Enums;
using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetConsentedAccountDetail;
public class GetConsentedAccountDetailQuery : IRequest<ConsentedAccountDetailResultDto>
{
    public string AccountReference { get; set; }
    public string AppUserId { get; set; }
    public string HhsCode { get; set; }
}

public class GetConsentedAccountDetailQueryHandler : IRequestHandler<GetConsentedAccountDetailQuery, ConsentedAccountDetailResultDto>
{
    private readonly IOpenBankingOperationsService _openBankingOperationsService;

    public GetConsentedAccountDetailQueryHandler(
         IOpenBankingOperationsService openBankingOperationsService)
    {
        _openBankingOperationsService = openBankingOperationsService;
    }

    public async Task<ConsentedAccountDetailResultDto> Handle(GetConsentedAccountDetailQuery request,
        CancellationToken cancellationToken)
    {
        return await _openBankingOperationsService.GetConsentedAccountDetailAsync(request);
    }
}

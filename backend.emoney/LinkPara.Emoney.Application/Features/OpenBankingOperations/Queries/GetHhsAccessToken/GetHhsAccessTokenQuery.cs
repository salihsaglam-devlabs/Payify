using LinkPara.ApiGateway.Services.Emoney.Models.Enums;
using LinkPara.Emoney.Application.Commons.Enums;
using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetHhsAccessToken;
public class GetHhsAccessTokenQuery : IRequest<YosServiceResultDto>
{

    public string ConsentId { get; set; }
    public ConsentType ConsentType { get; set; }
    public string AccessCode { get; set; }
}

public class GetHhsAccessTokenQueryHandler : IRequestHandler<GetHhsAccessTokenQuery, YosServiceResultDto>
{
    private readonly IOpenBankingOperationsService _openBankingOperationsService;

    public GetHhsAccessTokenQueryHandler(
         IOpenBankingOperationsService openBankingOperationsService)
    {
        _openBankingOperationsService = openBankingOperationsService;
    }

    public async Task<YosServiceResultDto> Handle(GetHhsAccessTokenQuery request,
        CancellationToken cancellationToken)
    {
        return await _openBankingOperationsService.GetHhsAccessTokenAsync(request);
    }
}

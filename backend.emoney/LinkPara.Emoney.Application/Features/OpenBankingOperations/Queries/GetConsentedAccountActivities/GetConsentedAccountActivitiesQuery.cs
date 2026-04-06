using LinkPara.ApiGateway.Services.Emoney.Models.Enums;
using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetConsentedAccountActivities;
public class GetConsentedAccountActivitiesQuery : IRequest<ConsentedAccountActivitiesResultDto>
{
    public string AppUserId { get; set; }
    public string HhsCode { get; set; }
    public string AccountReference { get; set; }
    public DateTime HesapIslemBslTrh { get; set; }
    public DateTime HesapIslemBtsTrh { get; set; }
    public string MinIslTtr { get; set; }
    public string MksIslTtr { get; set; }
    public string BrcAlc { get; set; }
    public string SyfNo { get; set; }
    public string SrlmKrtr { get; set; }
    public string SrlmYon { get; set; }
    public string SyfKytSayi { get; set; }
}

public class GetConsentedAccountActivitiesQueryHandler : IRequestHandler<GetConsentedAccountActivitiesQuery, ConsentedAccountActivitiesResultDto>
{
    private readonly IOpenBankingOperationsService _openBankingOperationsService;

    public GetConsentedAccountActivitiesQueryHandler(
         IOpenBankingOperationsService openBankingOperationsService)
    {
        _openBankingOperationsService = openBankingOperationsService;
    }

    public async Task<ConsentedAccountActivitiesResultDto> Handle(GetConsentedAccountActivitiesQuery request,
        CancellationToken cancellationToken)
    {
        return await _openBankingOperationsService.GetConsentedAccountActivitiesAsync(request);
    }
}

using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetCards;
public class GetCardsQuery : IRequest<CardsResultDto>
{
    public string HhsCode { get; set; }
    public string ApplicationUser { get; set; }
    public string ConsentId { get; set; }
}

public class GetCardsQueryHandler : IRequestHandler<GetCardsQuery, CardsResultDto>
{
    private readonly IOpenBankingOperationsService _openBankingOperationsService;

    public GetCardsQueryHandler(
         IOpenBankingOperationsService openBankingOperationsService)
    {
        _openBankingOperationsService = openBankingOperationsService;
    }

    public async Task<CardsResultDto> Handle(GetCardsQuery request,
        CancellationToken cancellationToken)
    {
        return await _openBankingOperationsService.GetCardsAsync(request);
    }
}

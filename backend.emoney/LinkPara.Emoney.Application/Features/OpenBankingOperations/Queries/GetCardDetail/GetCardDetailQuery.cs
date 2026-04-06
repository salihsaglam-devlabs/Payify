using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetCardDetail;
public class GetCardDetailQuery : IRequest<CardDetailResultDto>
{
    public string HhsCode { get; set; }
    public string AppUserId { get; set; }
    public string ConsentId { get; set; }
    public string CardRefNo { get; set; }
    public string StatementType { get; set; }
}

public class GetCardsQueryHandler : IRequestHandler<GetCardDetailQuery, CardDetailResultDto>
{
    private readonly IOpenBankingOperationsService _openBankingOperationsService;

    public GetCardsQueryHandler(
         IOpenBankingOperationsService openBankingOperationsService)
    {
        _openBankingOperationsService = openBankingOperationsService;
    }

    public async Task<CardDetailResultDto> Handle(GetCardDetailQuery request,
        CancellationToken cancellationToken)
    {
        return await _openBankingOperationsService.GetCardDetailAsync(request);
    }
}

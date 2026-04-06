using LinkPara.PF.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.PF.Application.Features.CardBins.Queries.GetCardBinByNumber;

public class GetCardBinByNumberQuery : IRequest<CardBinDto>
{
    public string BinNumber { get; set; }
}

public class GetCardBinByIdQueryHandler : IRequestHandler<GetCardBinByNumberQuery, CardBinDto>
{
    private readonly ICardBinService _cardBinService;

    public GetCardBinByIdQueryHandler(ICardBinService cardBinService)
    {
        _cardBinService = cardBinService;
    }
    public async Task<CardBinDto> Handle(GetCardBinByNumberQuery request, CancellationToken cancellationToken)
    {
        return await _cardBinService.GetByNumberAsync(request.BinNumber);
    }
}
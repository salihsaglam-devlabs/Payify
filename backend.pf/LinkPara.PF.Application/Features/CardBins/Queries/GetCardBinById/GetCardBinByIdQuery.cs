using LinkPara.PF.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.PF.Application.Features.CardBins.Queries.GetCardBinById;

public class GetCardBinByIdQuery : IRequest<CardBinDto>
{
    public Guid Id { get; set; }
}

public class GetCardBinByIdQueryHandler : IRequestHandler<GetCardBinByIdQuery, CardBinDto>
{
    private readonly ICardBinService _cardBinService;

    public GetCardBinByIdQueryHandler(ICardBinService cardBinService)
    {
        _cardBinService = cardBinService;
    }
    public async Task<CardBinDto> Handle(GetCardBinByIdQuery request, CancellationToken cancellationToken)
    {
        return await _cardBinService.GetByIdAsync(request);
    }
}

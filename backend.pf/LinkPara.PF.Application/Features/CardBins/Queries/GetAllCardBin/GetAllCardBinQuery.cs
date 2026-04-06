using AutoMapper;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.PF.Application.Features.CardBins.Queries.GetAllCardBin;

public class GetAllCardBinQuery : SearchQueryParams, IRequest<PaginatedList<CardBinDto>>
{
    public CardBrand? CardBrand { get; set; }
    public CardType? CardType { get; set; }
    public CardNetwork? CardNetwork { get; set; }
    public CardSubType? CardSubType { get; set; }
    public RecordStatus? RecordStatus { get; set; }
}

public class GetAllCardBinQueryHandler : IRequestHandler<GetAllCardBinQuery, PaginatedList<CardBinDto>>
{
    private readonly ICardBinService _cardBinService;

    public GetAllCardBinQueryHandler(ICardBinService cardBinService)
    {
        _cardBinService = cardBinService;
    }
    public async Task<PaginatedList<CardBinDto>> Handle(GetAllCardBinQuery request, CancellationToken cancellationToken)
    {
        return await _cardBinService.GetListAsync(request);
    }
}

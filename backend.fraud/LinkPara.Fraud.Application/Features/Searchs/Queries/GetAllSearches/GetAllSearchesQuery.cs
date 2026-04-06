using LinkPara.Fraud.Application.Commons.Interfaces;
using LinkPara.Fraud.Application.Features.Searchs;
using LinkPara.Fraud.Domain.Enums;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.Fraud.Application.Features.Transactions.Queries.GetAllSearches;

public class GetAllSearchesQuery : SearchQueryParams, IRequest<PaginatedList<SearchLogDto>> 
{
    public string SearchName { get; set; }
    public SearchType? SearchType { get; set; }
    public MatchStatus? MatchStatus { get; set; }
    public bool? IsBlackList { get; set; }
    public DateTime? DateStart { get; set; }
    public DateTime? DateEnd { get; set; }
}

public class GetAllSearchesQueryHandler : IRequestHandler<GetAllSearchesQuery, PaginatedList<SearchLogDto>>
{
    private readonly ISearchService _searchService;

    public GetAllSearchesQueryHandler(ISearchService searchService)
    {
        _searchService = searchService;
    }
    public async Task<PaginatedList<SearchLogDto>> Handle(GetAllSearchesQuery request, CancellationToken cancellationToken)
    {
        return await _searchService.GetListAsync(request);
    }
}

using LinkPara.Fraud.Application.Commons.Interfaces;
using LinkPara.Fraud.Application.Commons.Models.Searchs;
using MediatR;

namespace LinkPara.Fraud.Application.Features.Searchs.SearchByIdentities;

public class SearchByIdentityQuery : IRequest<SearchResponse>
{
    public string Id { get; set; }
}
public class SearchByIdentityQueryHandler : IRequestHandler<SearchByIdentityQuery, SearchResponse>
{
    private readonly ISearchService _searchService;

    public SearchByIdentityQueryHandler(ISearchService searchService)
    {
        _searchService = searchService;
    }

    public async Task<SearchResponse> Handle(SearchByIdentityQuery request, CancellationToken cancellationToken)
    {
        return await _searchService.GetSearchByIdentityAsync(request.Id);
    }
}
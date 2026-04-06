using LinkPara.Fraud.Application.Commons.Interfaces;
using LinkPara.Fraud.Application.Commons.Models.Searchs;
using LinkPara.Fraud.Domain.Enums;
using LinkPara.SharedModels.Boa.Enums;
using MediatR;

namespace LinkPara.Fraud.Application.Features.Searchs.SearchByNames;

public class SearchByNameQuery : IRequest<SearchResponse>
{
    public string Name { get; set; }
    public string BirthYear { get; set; }
    public SearchType SearchType { get; set; }
    public FraudChannelType FraudChannelType { get; set; }
}

public class SearchByNameQueryHandler : IRequestHandler<SearchByNameQuery, SearchResponse>
{
    private readonly ISearchService _searchService;

    public SearchByNameQueryHandler(ISearchService searchService)
    {
        _searchService = searchService;
    }

    public async Task<SearchResponse> Handle(SearchByNameQuery request, CancellationToken cancellationToken)
    {
        return await _searchService.GetSearchByNameAsync(request);
    }
}

using LinkPara.HttpProviders.Fraud.Models;

namespace LinkPara.HttpProviders.Fraud;

public interface ISearchService
{
    Task<SearchResponse> GetSearchByName(SearchByNameRequest request);
    Task<SearchResponse> GetSearchByIdentity(string id);
}

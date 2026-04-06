using LinkPara.HttpProviders.Fraud.Models;
using LinkPara.HttpProviders.Utility;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;
using System.Web;

namespace LinkPara.HttpProviders.Fraud;

public class SearchService : HttpClientBase, ISearchService
{

    public SearchService(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {

    }

    public async Task<SearchResponse> GetSearchByIdentity(string id)
    {
        var response = await GetAsync($"v1/Search/{id}");

        var searchResponse = await response.Content.ReadFromJsonAsync<SearchResponse>();

        return searchResponse ?? throw new InvalidOperationException();
    }

    public async Task<SearchResponse> GetSearchByName(SearchByNameRequest request)
    {
        var url = GetQueryString.CreateUrlWithParams($"v1/Search", request);

        var response = await GetAsync(url);

        var searchResponse = await response.Content.ReadFromJsonAsync<SearchResponse>();

        return searchResponse ?? throw new InvalidOperationException();
    }
}

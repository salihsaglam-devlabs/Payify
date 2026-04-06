using LinkPara.ApiGateway.Services.Fraud.Models.Request;
using LinkPara.ApiGateway.Services.Fraud.Models.Response;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Services.Fraud.HttpClients
{
    public class SearchClient : HttpClientBase, ISearchClient
    {
        public SearchClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
      : base(client, httpContextAccessor)
        {
        }

        public async Task<PaginatedList<SearchLogResponse>> GetAllAsync(GetAllSearchesRequest request)
        {
            var url = CreateUrlWithParams($"v1/Search/logs", request, true);
            var response = await GetAsync(url);
            var searchLogResponse = await response.Content.ReadFromJsonAsync<PaginatedList<SearchLogResponse>>();
            return searchLogResponse ?? throw new InvalidOperationException();
        }

        public async Task<PaginatedList<SearchLogResponse>> GetSearchByNameAsync(SearchByNameRequest request)
        {
            var url = CreateUrlWithParams($"v1/Search", request, true);
            var response = await GetAsync(url);
            var searchLogResponse = await response.Content.ReadFromJsonAsync<PaginatedList<SearchLogResponse>>();
            return searchLogResponse ?? throw new InvalidOperationException();
        }
    }
}

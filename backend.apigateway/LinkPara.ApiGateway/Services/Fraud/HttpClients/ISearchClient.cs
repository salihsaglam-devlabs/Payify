using LinkPara.ApiGateway.Services.Fraud.Models.Request;
using LinkPara.ApiGateway.Services.Fraud.Models.Response;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Services.Fraud.HttpClients
{
    public interface ISearchClient
    {
        public Task<PaginatedList<SearchLogResponse>> GetAllAsync(GetAllSearchesRequest request);
        public Task<PaginatedList<SearchLogResponse>> GetSearchByNameAsync(SearchByNameRequest request);
    }
}

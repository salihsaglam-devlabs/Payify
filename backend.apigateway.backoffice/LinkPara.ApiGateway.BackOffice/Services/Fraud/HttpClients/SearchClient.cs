using DocumentFormat.OpenXml.Office2010.Excel;
using LinkPara.ApiGateway.BackOffice.Commons.Models;
using LinkPara.ApiGateway.BackOffice.Services.Fraud.Models.Request;
using LinkPara.ApiGateway.BackOffice.Services.Fraud.Models.Response;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Fraud.HttpClients
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

        public async Task<PaginatedList<OngoingMonitoringResponse>> GetAllOngoingMonitoringAsync(GetAllOngoingMonitoringRequest request)
        {
            var url = CreateUrlWithParams($"v1/Search/ongoingMonitorings", request, true);
            var response = await GetAsync(url);
            var ongoingMonitoringResponse = await response.Content.ReadFromJsonAsync<PaginatedList<OngoingMonitoringResponse>>();
            return ongoingMonitoringResponse ?? throw new InvalidOperationException();
        }

        public async Task<BaseResponse> RemoveOngoingMonitoringAsync(string referenceNumber)
        {
            var response = await PutAsJsonAsync($"v1/Search/removeOngoingMonitoring", referenceNumber);
            var ongoingMonitoringResponse = await response.Content.ReadFromJsonAsync<BaseResponse>();
            return ongoingMonitoringResponse ?? throw new InvalidOperationException();
        }
    }
}

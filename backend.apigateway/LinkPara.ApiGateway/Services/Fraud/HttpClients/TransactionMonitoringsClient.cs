using LinkPara.ApiGateway.Services.Fraud.Models.Request;
using LinkPara.ApiGateway.Services.Fraud.Models.Response;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Services.Fraud.HttpClients
{
    public class TransactionMonitoringsClient: HttpClientBase, ITransactionMonitoringsClient
    {
        public TransactionMonitoringsClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
            : base(client, httpContextAccessor)
        {
        }

        public async Task<PaginatedList<TransactionMonitoringResponse>> GetAllAsync(GetAllTransactionMonitoringRequest request)
        {
            var url = CreateUrlWithParams($"v1/TransactionMonitorings", request, true);
            var response = await GetAsync(url);
            var transactionMonitoringResponse = await response.Content.ReadFromJsonAsync<PaginatedList<TransactionMonitoringResponse>>();
            return transactionMonitoringResponse ?? throw new InvalidOperationException();
        }
    }
}

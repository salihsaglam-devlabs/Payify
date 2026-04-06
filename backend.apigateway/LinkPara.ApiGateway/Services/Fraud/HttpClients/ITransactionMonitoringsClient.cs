using LinkPara.ApiGateway.Services.Fraud.Models.Request;
using LinkPara.ApiGateway.Services.Fraud.Models.Response;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Services.Fraud.HttpClients
{
    public interface ITransactionMonitoringsClient
    {
        public Task<PaginatedList<TransactionMonitoringResponse>> GetAllAsync(GetAllTransactionMonitoringRequest request);
    }
}

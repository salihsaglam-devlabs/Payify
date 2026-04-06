using LinkPara.ApiGateway.BackOffice.Services.Fraud.Models.Request;
using LinkPara.ApiGateway.BackOffice.Services.Fraud.Models.Response;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Fraud.HttpClients
{
    public interface ITransactionMonitoringsClient
    {
        public Task<PaginatedList<TransactionMonitoringResponse>> GetAllAsync(GetAllTransactionMonitoringRequest request);
    }
}

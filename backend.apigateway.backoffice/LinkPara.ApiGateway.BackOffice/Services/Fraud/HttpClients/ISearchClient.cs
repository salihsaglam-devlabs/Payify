using LinkPara.ApiGateway.BackOffice.Commons.Models;
using LinkPara.ApiGateway.BackOffice.Services.Fraud.Models.Request;
using LinkPara.ApiGateway.BackOffice.Services.Fraud.Models.Response;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Fraud.HttpClients
{
    public interface ISearchClient
    {
        public Task<PaginatedList<SearchLogResponse>> GetAllAsync(GetAllSearchesRequest request);
        public Task<PaginatedList<OngoingMonitoringResponse>> GetAllOngoingMonitoringAsync(GetAllOngoingMonitoringRequest request);
        public Task<BaseResponse> RemoveOngoingMonitoringAsync(string referenceNumber);
    }
}

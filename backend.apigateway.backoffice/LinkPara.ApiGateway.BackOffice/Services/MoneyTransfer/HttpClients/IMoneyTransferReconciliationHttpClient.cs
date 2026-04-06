using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests.MoneyTransferReconciliation;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses.MoneyTransferReconciliation;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients
{
    public interface IMoneyTransferReconciliationHttpClient
    {
        Task<PaginatedList<ReconciliationSummaryDto>> GetReconciliationSummaryAsync(ReconciliationSummaryRequest request);
        Task<PaginatedList<ReconciliationDetailDto>> GetReconciliationDetailAsync(ReconciliationDetailRequest request);
        Task CancelReconciliationDetailAsync(CancelReconciliationDetailRequest request);
        Task RunReconciliationAsync(RunReconciliationRequest request);
    }
}

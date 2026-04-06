using LinkPara.ApiGateway.BackOffice.Services.Epin.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Epin.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Epin.HttpClients;

public interface IEpinReconciliationHttpClient
{
    Task<PaginatedList<EpinReconciliationSummaryDto>> GetFilterReconciliationSummariesAsync(GetFilterReconciliationSummariesRequest request);
    Task<EpinReconciliationSummaryDto> GetReconciliationSummaryAsync(Guid id);
    Task ReconciliationByDateAsync(ReconciliationByDateRequest request);
}

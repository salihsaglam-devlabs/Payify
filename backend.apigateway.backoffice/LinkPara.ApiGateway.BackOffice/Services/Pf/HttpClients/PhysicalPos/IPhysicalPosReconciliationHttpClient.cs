using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests.PhysicalPos;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses.PhysicalPos;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients.PhysicalPos;

public interface IPhysicalPosReconciliationHttpClient
{
    Task<PaginatedList<PhysicalPosEndOfDayDto>> GetAllPhysicalPosEndOfDayAsync(GetAllPhysicalPosEndOfDayRequest request);
    Task<PhysicalPosEndOfDayDetailResponse> GetDetailsByIdAsync(Guid id);
    Task<HttpResponseMessage> DownloadReconciliationReportWithBytesAsync(Guid id);
    Task BatchManualReconciliationAsync(BatchManualReconciliationRequest request);
    Task SingleManualReconciliationAsync(SingleManualReconciliationRequest request);
}
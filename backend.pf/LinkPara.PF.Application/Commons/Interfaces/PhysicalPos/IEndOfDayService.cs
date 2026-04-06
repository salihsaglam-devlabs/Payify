using LinkPara.PF.Application.Features.PhysicalPos.Reconciliation;
using LinkPara.PF.Application.Features.PhysicalPos.Reconciliation.Queries.GetAllPhysicalPosEndOfDay;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.PF.Application.Commons.Interfaces.PhysicalPos;

public interface IEndOfDayService
{
    Task<PaginatedList<PhysicalPosEndOfDayDto>> GetAllPhysicalPosEndOfDayAsync(GetAllPhysicalPosEndOfDayQuery request);
    Task<PhysicalPosEndOfDayDetailResponse> GetDetailsByIdAsync(Guid id);
    Task<IActionResult> DownloadEndOfDayDetailByIdAsync(Guid id);
    Task BatchManualReconciliationAsync(Guid endOfDayId);
    Task SingleManualReconciliationAsync(Guid merchantTransactionId);
}
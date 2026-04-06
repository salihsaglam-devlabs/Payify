using LinkPara.PF.Application.Features.DeviceInventoryHistories;
using LinkPara.PF.Application.Features.DeviceInventoryHistories.Queries.GetAllDeviceInventoryHistories;
using LinkPara.PF.Domain.Enums.PhysicalPos;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface IDeviceInventoryHistoryService
{
    Task<PaginatedList<DeviceInventoryHistoryDto>> GetAllAsync(GetAllDeviceInventoryHistoryQuery request);
    Task CreateSnapshotHistory(Guid deviceInventoryId, DeviceHistoryType historyType, string oldData, string newData, string detail, string createdBy);
}

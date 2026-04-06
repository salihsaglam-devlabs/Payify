using LinkPara.PF.Application.Features.DeviceInventories;
using LinkPara.PF.Application.Features.DeviceInventories.Command.DeleteDeviceInventory;
using LinkPara.PF.Application.Features.DeviceInventories.Command.SaveDeviceInventory;
using LinkPara.PF.Application.Features.DeviceInventories.Command.UpdateDeviceInventory;
using LinkPara.PF.Application.Features.DeviceInventories.Queries.GetAllDeviceInventories;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface IDeviceInventoryService
{
    Task<PaginatedList<DeviceInventoryDto>> GetAllAsync(GetAllDeviceInventoryQuery request);
    Task<DeviceInventoryDto> GetByIdAsync(Guid id);
    Task DeleteAsync(DeleteDeviceInventoryCommand command);
    Task SaveAsync(SaveDeviceInventoryCommand command);
    Task UpdateAsync(UpdateDeviceInventoryCommand command);
}

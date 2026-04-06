using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests.PhysicalPos;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses.PhysicalPos;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients.PhysicalPos;

public interface IDeviceInventoryHttpClient
{
    Task<PaginatedList<DeviceInventoryDto>> GetAllAsync(GetAllDeviceInventoryRequest request);
    Task<PaginatedList<DeviceInventoryHistoryDto>> GetAllDeviceHistoryAsync(GetAllDeviceInventoryHistoryRequest request);
    Task<DeviceInventoryDto> GetByIdAsync(Guid id);
    Task SaveAsync(SaveDeviceInventoryRequest request);
    Task UpdateAsync(UpdateDeviceInventoryRequest request);
    Task DeleteDeviceInventoryAsync(Guid id);
}

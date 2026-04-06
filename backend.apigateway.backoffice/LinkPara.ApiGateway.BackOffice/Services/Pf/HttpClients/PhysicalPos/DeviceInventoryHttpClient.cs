using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests.PhysicalPos;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses.PhysicalPos;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients.PhysicalPos;

public class DeviceInventoryHttpClient : HttpClientBase, IDeviceInventoryHttpClient
{
    public DeviceInventoryHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
    {
    }

    public async Task DeleteDeviceInventoryAsync(Guid id)
    {
        var response = await DeleteAsync($"v1/DeviceInventory/{id}");
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task<PaginatedList<DeviceInventoryDto>> GetAllAsync(GetAllDeviceInventoryRequest request)
    {
        var url = CreateUrlWithParams($"v1/DeviceInventory", request, true);
        var response = await GetAsync(url);
        var deviceInventories = await response.Content.ReadFromJsonAsync<PaginatedList<DeviceInventoryDto>>();
        return deviceInventories ?? throw new InvalidOperationException();
    }

    public async Task<PaginatedList<DeviceInventoryHistoryDto>> GetAllDeviceHistoryAsync(GetAllDeviceInventoryHistoryRequest request)
    {
        var url = CreateUrlWithParams($"v1/DeviceInventory/deviceInventoryHistories", request, true);
        var response = await GetAsync(url);
        var deviceInventoryHistories = await response.Content.ReadFromJsonAsync<PaginatedList<DeviceInventoryHistoryDto>>();
        return deviceInventoryHistories ?? throw new InvalidOperationException();
    }

    public async Task<DeviceInventoryDto> GetByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/DeviceInventory/{id}");
        var deviceInventory = await response.Content.ReadFromJsonAsync<DeviceInventoryDto>();
        return deviceInventory ?? throw new InvalidOperationException();
    }

    public async Task SaveAsync(SaveDeviceInventoryRequest request)
    {
        var response = await PostAsJsonAsync($"v1/DeviceInventory", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task UpdateAsync(UpdateDeviceInventoryRequest request)
    {
        var response = await PutAsJsonAsync($"v1/DeviceInventory", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }
}

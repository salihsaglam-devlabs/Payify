using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests.PhysicalPos;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses.PhysicalPos;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients.PhysicalPos;

public class MerchantPhysicalDeviceClient : HttpClientBase, IMerchantPhysicalDeviceClient
{
    public MerchantPhysicalDeviceClient(HttpClient client, IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
    {
    }

    public async Task DeleteMerchantPhysicalDeviceAsync(UpdateMerchantPhysicalDeviceRequest request)
    {
        var response = await PutAsJsonAsync($"v1/MerchantPhysicalDevice", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task DeleteMerchantPhysicalPosAsync(Guid id)
    {
        var response = await DeleteAsync($"v1/MerchantPhysicalDevice/{id}");
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task<PaginatedList<MerchantPhysicalDeviceDto>> GetAllAsync(GetAllMerchantPhysicalDeviceRequest request)
    {
        var url = CreateUrlWithParams($"v1/MerchantPhysicalDevice", request, true);
        var response = await GetAsync(url);
        var merchantPhysicalDevices = await response.Content.ReadFromJsonAsync<PaginatedList<MerchantPhysicalDeviceDto>>();
        return merchantPhysicalDevices ?? throw new InvalidOperationException();
    }

    public async Task SaveAsync(SaveMerchantPhysicalDeviceRequest request)
    {
        var response = await PostAsJsonAsync($"v1/MerchantPhysicalDevice", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task SaveMerchantPhysicalPosAsync(SaveMerchantPhysicalPosRequest request)
    {
        var response = await PostAsJsonAsync($"v1/MerchantPhysicalDevice/saveMerchantPhysicalPos", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }
    
    public async Task<List<DeviceApiKeyDecryptedDto>> GetAllDeviceApiKeysAsync(Guid merchantId)
    {
        var response = await GetAsync($"v1/MerchantPhysicalDevice/decrypted-api-keys/{merchantId}");
        var apiKeys = await response.Content.ReadFromJsonAsync<List<DeviceApiKeyDecryptedDto>>();
        return apiKeys ?? throw new InvalidOperationException();
    }
}

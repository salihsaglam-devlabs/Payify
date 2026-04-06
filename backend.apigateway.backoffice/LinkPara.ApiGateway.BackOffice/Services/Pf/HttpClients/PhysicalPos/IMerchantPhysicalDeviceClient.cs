using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests.PhysicalPos;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses.PhysicalPos;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients.PhysicalPos;

public interface IMerchantPhysicalDeviceClient
{
    Task<PaginatedList<MerchantPhysicalDeviceDto>> GetAllAsync(GetAllMerchantPhysicalDeviceRequest request);
    Task SaveAsync(SaveMerchantPhysicalDeviceRequest request);
    Task SaveMerchantPhysicalPosAsync(SaveMerchantPhysicalPosRequest request);
    Task DeleteMerchantPhysicalDeviceAsync(UpdateMerchantPhysicalDeviceRequest request);
    Task DeleteMerchantPhysicalPosAsync(Guid id);
    Task<List<DeviceApiKeyDecryptedDto>> GetAllDeviceApiKeysAsync(Guid merchantId);
}

using LinkPara.PF.Application.Features.MerchantPhysicalDevices;
using LinkPara.PF.Application.Features.MerchantPhysicalDevices.Command.DeleteMerchantPhysicalPos;
using LinkPara.PF.Application.Features.MerchantPhysicalDevices.Command.SaveMerchantPhysicalDevice;
using LinkPara.PF.Application.Features.MerchantPhysicalDevices.Command.SaveMerchantPhysicalPos;
using LinkPara.PF.Application.Features.MerchantPhysicalDevices.Command.UpdateMerchantPhysicalDevice;
using LinkPara.PF.Application.Features.MerchantPhysicalDevices.Queries.GetAllMerchantPhysicalDevice;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface IMerchantPhysicalDeviceService
{
    Task<PaginatedList<MerchantPhysicalDeviceDto>> GetAllAsync(GetAllMerchantPhysicalDeviceQuery request);
    Task SaveAsync(SaveMerchantPhysicalDeviceCommand command);
    Task UpdateAsync(UpdateMerchantPhysicalDeviceCommand command);
    Task SaveMerchantPhysicalPosAsync(SaveMerchantPhysicalPosCommand command);
    Task DeleteMerchantPhysicalPosAsync(DeleteMerchantPhysicalPosCommand command);
}

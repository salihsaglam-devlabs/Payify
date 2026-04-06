using LinkPara.PF.Application.Features.MerchantPhysicalDevices;
using LinkPara.PF.Application.Features.MerchantPhysicalDevices.Command.DeleteMerchantPhysicalPos;
using LinkPara.PF.Application.Features.MerchantPhysicalDevices.Command.SaveMerchantPhysicalDevice;
using LinkPara.PF.Application.Features.MerchantPhysicalDevices.Command.SaveMerchantPhysicalPos;
using LinkPara.PF.Application.Features.MerchantPhysicalDevices.Command.UpdateMerchantPhysicalDevice;
using LinkPara.PF.Application.Features.MerchantPhysicalDevices.Queries.GetAllDeviceApiKeys;
using LinkPara.PF.Application.Features.MerchantPhysicalDevices.Queries.GetAllMerchantPhysicalDevice;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.PF.API.Controllers;

public class MerchantPhysicalDeviceController : ApiControllerBase
{

    /// <summary>
    /// Returns all merchant physical devices.
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "MerchantPhysicalDevice:ReadAll")]
    [HttpGet("")]
    public async Task<PaginatedList<MerchantPhysicalDeviceDto>> GetAllAsync([FromQuery] GetAllMerchantPhysicalDeviceQuery request)
    {
        return await Mediator.Send(request);
    }

    /// <summary>
    /// Creates a merchant physical device.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantPhysicalDevice:Create")]
    [HttpPost("")]
    public async Task SaveAsync(SaveMerchantPhysicalDeviceCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Creates a merchant physical pos.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantPhysicalDevice:Create")]
    [HttpPost("saveMerchantPhysicalPos")]
    public async Task SaveMerchantPhysicalPosAsync(SaveMerchantPhysicalPosCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Updates a merchant physical device.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantPhysicalDevice:Update")]
    [HttpPut("")]
    public async Task UpdateAsync(UpdateMerchantPhysicalDeviceCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Delete an merchant physical pos.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantPhysicalDevice:Delete")]
    [HttpDelete("{id}")]
    public async Task DeleteAsync(Guid id)
    {
        await Mediator.Send(new DeleteMerchantPhysicalPosCommand { MerchantPhysicalPosId = id });
    }
    
    /// <summary>
    /// Returns all merchant physical device api keys.
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "MerchantPhysicalDevice:ReadAll")]
    [HttpGet("decrypted-api-keys/{merchantId}")]
    public async Task<List<DeviceApiKeyDecryptedDto>> GetAllApiKeysAsync([FromRoute] Guid merchantId)
    {
        return await Mediator.Send(new GetAllDeviceApiKeysQuery { MerchantId = merchantId });
    }
}

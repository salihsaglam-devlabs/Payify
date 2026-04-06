using LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients.PhysicalPos;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests.PhysicalPos;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses.PhysicalPos;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Pf;

public class MerchantPhysicalDeviceController : ApiControllerBase
{
    private readonly IMerchantPhysicalDeviceClient _merchantPhysicalDeviceClient;
    public MerchantPhysicalDeviceController(IMerchantPhysicalDeviceClient merchantPhysicalDeviceClient)
    {
        _merchantPhysicalDeviceClient = merchantPhysicalDeviceClient;
    }

    /// <summary>
    /// Returns all merchant physical devices.
    /// </summary>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "MerchantPhysicalDevice:ReadAll")]
    public async Task<ActionResult<PaginatedList<MerchantPhysicalDeviceDto>>> GetAllAsync([FromQuery] GetAllMerchantPhysicalDeviceRequest request)
    {
        return await _merchantPhysicalDeviceClient.GetAllAsync(request);
    }

    /// <summary>
    /// Creates a merchant physical device.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("")]
    [Authorize(Policy = "MerchantPhysicalDevice:Create")]
    public async Task SaveAsync(SaveMerchantPhysicalDeviceRequest request)
    {
        await _merchantPhysicalDeviceClient.SaveAsync(request);
    }

    /// <summary>
    /// Creates a merchant physical pos.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("saveMerchantPhysicalPos")]
    [Authorize(Policy = "MerchantPhysicalDevice:Create")]
    public async Task SaveMerchantPhysicalPosAsync(SaveMerchantPhysicalPosRequest request)
    {
        await _merchantPhysicalDeviceClient.SaveMerchantPhysicalPosAsync(request);
    }

    /// <summary>
    /// Updates a merchant physical device.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("")]
    [Authorize(Policy = "MerchantPhysicalDevice:Update")]
    public async Task UpdateAsync(UpdateMerchantPhysicalDeviceRequest request)
    {
        await _merchantPhysicalDeviceClient.DeleteMerchantPhysicalDeviceAsync(request);
    }

    /// <summary>
    /// Delete a merchant physical pos.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("")]
    [Authorize(Policy = "MerchantPhysicalDevice:Delete")]
    public async Task DeleteAsync(Guid id)
    {
        await _merchantPhysicalDeviceClient.DeleteMerchantPhysicalPosAsync(id);
    }
    
    /// <summary>
    /// Returns all merchant physical device api keys.
    /// </summary>
    /// <returns></returns>
    [HttpGet("decrypted-api-keys/{merchantId}")]
    [Authorize(Policy = "MerchantPhysicalDevice:ReadAll")]
    public async Task<ActionResult<List<DeviceApiKeyDecryptedDto>>> GetAllApiKeysAsync([FromRoute] Guid merchantId)
    {
        return await _merchantPhysicalDeviceClient.GetAllDeviceApiKeysAsync(merchantId);
    }
}

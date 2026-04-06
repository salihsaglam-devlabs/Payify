using LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients.PhysicalPos;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests.PhysicalPos;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses.PhysicalPos;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Pf;

public class DeviceInventoryController : ApiControllerBase
{
    private readonly IDeviceInventoryHttpClient _deviceInventoryHttpClient;
    public DeviceInventoryController(IDeviceInventoryHttpClient deviceInventoryHttpClient)
    {
        _deviceInventoryHttpClient = deviceInventoryHttpClient;
    }

    /// <summary>
    /// Returns all device inventories
    /// </summary>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "DeviceInventory:ReadAll")]
    public async Task<ActionResult<PaginatedList<DeviceInventoryDto>>> GetAllAsync([FromQuery] GetAllDeviceInventoryRequest request)
    {
        return await _deviceInventoryHttpClient.GetAllAsync(request);
    }

    /// <summary>
    /// Returns a device inventory.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "DeviceInventory:Read")]
    public async Task<ActionResult<DeviceInventoryDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await _deviceInventoryHttpClient.GetByIdAsync(id);
    }

    /// <summary>
    /// Creates a device inventory.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("")]
    [Authorize(Policy = "DeviceInventory:Create")]
    public async Task SaveAsync(SaveDeviceInventoryRequest request)
    {
        await _deviceInventoryHttpClient.SaveAsync(request);
    }

    /// <summary>
    /// Updates a device inventory.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("")]
    [Authorize(Policy = "DeviceInventory:Update")]
    public async Task UpdateAsync(UpdateDeviceInventoryRequest request)
    {
        await _deviceInventoryHttpClient.UpdateAsync(request);
    }

    /// <summary>
    /// Delete a device inventory.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("")]
    [Authorize(Policy = "DeviceInventory:Delete")]
    public async Task DeleteAsync(Guid id)
    {
        await _deviceInventoryHttpClient.DeleteDeviceInventoryAsync(id);
    }

    /// <summary>
    /// Returns all device inventory histories
    /// </summary>
    /// <returns></returns>
    [HttpGet("deviceInventoryHistories")]
    [Authorize(Policy = "DeviceInventory:ReadAll")]
    public async Task<ActionResult<PaginatedList<DeviceInventoryHistoryDto>>> GetAllDeviceHistoryAsync([FromQuery] GetAllDeviceInventoryHistoryRequest request)
    {
        return await _deviceInventoryHttpClient.GetAllDeviceHistoryAsync(request);
    }
}

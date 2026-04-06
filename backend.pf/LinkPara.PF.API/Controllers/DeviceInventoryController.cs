using LinkPara.PF.Application.Features.DeviceInventories;
using LinkPara.PF.Application.Features.DeviceInventories.Command.DeleteDeviceInventory;
using LinkPara.PF.Application.Features.DeviceInventories.Command.SaveDeviceInventory;
using LinkPara.PF.Application.Features.DeviceInventories.Command.UpdateDeviceInventory;
using LinkPara.PF.Application.Features.DeviceInventories.Queries.GetAllDeviceInventories;
using LinkPara.PF.Application.Features.DeviceInventories.Queries.GetDeviceInventoryById;
using LinkPara.PF.Application.Features.DeviceInventoryHistories;
using LinkPara.PF.Application.Features.DeviceInventoryHistories.Queries.GetAllDeviceInventoryHistories;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.PF.API.Controllers;

public class DeviceInventoryController : ApiControllerBase
{
    /// <summary>
    /// Returns a DeviceInventory 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "DeviceInventory:Read")]
    [HttpGet("{id}")]
    public async Task<ActionResult<DeviceInventoryDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetDeviceInventoryByIdQuery { Id = id });
    }

    /// <summary>
    /// Returns filtered Device Inventories 
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "DeviceInventory:ReadAll")]
    [HttpGet("")]
    public async Task<ActionResult<PaginatedList<DeviceInventoryDto>>> GetFilterAsync([FromQuery] GetAllDeviceInventoryQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Creates a new Device Inventory
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "DeviceInventory:Create")]
    [HttpPost("")]
    public async Task SaveAsync(SaveDeviceInventoryCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Updates Device Inventory 
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "DeviceInventory:Update")]
    [HttpPut("")]
    public async Task UpdateAsync(UpdateDeviceInventoryCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Delete Device Inventory
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "DeviceInventory:Delete")]
    [HttpDelete("{id}")]
    public async Task DeleteAsync(Guid id)
    {
        await Mediator.Send(new DeleteDeviceInventoryCommand { Id = id });
    }

    /// <summary>
    /// Returns filtered Device Inventory Histories
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    //[Authorize(Policy = "DeviceInventory:ReadAll")]
    [HttpGet("deviceInventoryHistories")]
    public async Task<ActionResult<PaginatedList<DeviceInventoryHistoryDto>>> GetAllHistoryAsync([FromQuery] GetAllDeviceInventoryHistoryQuery query)
    {
        return await Mediator.Send(query);
    }
}

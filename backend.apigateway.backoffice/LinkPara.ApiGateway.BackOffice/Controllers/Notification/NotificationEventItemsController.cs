using LinkPara.ApiGateway.BackOffice.Services.Notification.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Notification;

public class NotificationEventItemsController : ApiControllerBase
{
    private readonly INotificationEventItemsHttpClient _client;
    
    public NotificationEventItemsController(INotificationEventItemsHttpClient client)
    {
        _client = client;
    }
    
    // <summary>
    /// Creates a new notification event item
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "Notification:Create")]
    [HttpPost("")]
    public async Task<ActionResult> CreateAsync(CreateNotificationEventItemRequest command)
    {
        await _client.CreateAsync(command);
        return NoContent();
    }
    
    /// <summary>
    /// Updates a notification event item
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "Notification:Update")]
    [HttpPut("")]
    public async Task<ActionResult> UpdateAsync(UpdateNotificationEventItemRequest command)
    {
        await _client.UpdateAsync(command);
        return NoContent();
    }
    
    /// <summary>
    /// Returns all notification  event items with filters
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "Notification:ReadAll")]
    [HttpGet("")]
    public async Task<ActionResult<PaginatedList<NotificationEventItemDto>>> GetAllAsync([FromQuery] GetAllNotificationEventItemRequest query)
    {
        return await _client.GetAllAsync(query);
    }
    
    /// <summary>
    /// Returns all notification event item by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "Notification:Read")]
    [HttpGet("{id}")]
    public async Task<ActionResult<NotificationEventItemDto>> GetNotificationEventItemById([FromRoute] Guid id)
    {
        return await _client.GetById(id);
    }
    
    /// <summary>
    /// deletes notification  event item with id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "Notification:Delete")]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteNotificationItemById(Guid id)
    {
        await _client.DeleteTemplateById(id);
        return NoContent();
    }
}
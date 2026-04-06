using LinkPara.ApiGateway.BackOffice.Services.Notification.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Notification;

public class NotificationItemsController : ApiControllerBase
{
    private readonly INotificationItemsHttpClient _httpClient;

    public NotificationItemsController(INotificationItemsHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Creates a new notification
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Notification:Create")]
    [HttpPost("")]
    public async Task CreateAsync(CreateNotificationItemRequest request)
    {
        await _httpClient.CreateAsync(request);
    }
    
    /// <summary>
    /// Updates a notification
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Notification:Update")]
    [HttpPut("")]
    public async Task UpdateAsync(UpdateNotificationItemRequest request)
    {
        await _httpClient.UpdateAsync(request);
    }
    
    /// <summary>
    /// Returns all notifications with filters
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Notification:ReadAll")]
    [HttpGet("")]
    public async Task<PaginatedList<NotificationItemDto>> GetAllAsync([FromQuery] GetAllNotificationItemsRequest request)
    {
        return await _httpClient.GetAllAsync(request);
    }
    
    /// <summary>
    /// Returns all notifications with filters
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "Notification:Read")]
    [HttpGet("{id}")]
    public async Task<NotificationItemDto> GetNotificationItemById([FromRoute] Guid id)
    {
        return await _httpClient.GetNotificationItemById(id);
    }
    
    /// <summary>
    /// deletes notification item with id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "Notification:Delete")]
    [HttpDelete("{id}")]
    public async Task DeleteNotificationItemById(Guid id)
    {
        await _httpClient.DeleteNotificationItemById(id);
    }
}
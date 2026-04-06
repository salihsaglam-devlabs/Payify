using LinkPara.ApiGateway.BackOffice.Services.Notification.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Notification;

public class NotificationOrdersController : ApiControllerBase
{
    private readonly INotificationOrdersHttpClient _httpClient;

    public NotificationOrdersController(INotificationOrdersHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Returns all notifications order with filters
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "Order:ReadAll")]
    [HttpGet("")]
    public async Task<PaginatedList<NotificationOrderDto>> GetAllAsync([FromQuery] GetAllNotificationOrdersRequest request)
    {
        return await _httpClient.GetAllAsync(request);
    }
    
    /// <summary>
    /// Returns all notifications order histories with order id
    /// </summary>
    /// <param name="orderId"></param>
    /// <returns></returns>
    [Authorize(Policy = "Order:ReadAll")]
    [HttpGet("history/{orderId}")]
    public async Task<PaginatedList<NotificationOrderHistoryDto>> GetNotificationOrderHistoriesByOrderId([FromRoute] Guid orderId)
    {
        return await _httpClient.GetNotificationOrderHistoriesByOrderId(orderId);
    }
}
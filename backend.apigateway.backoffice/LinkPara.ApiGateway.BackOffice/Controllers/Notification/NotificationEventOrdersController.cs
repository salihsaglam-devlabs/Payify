using LinkPara.ApiGateway.BackOffice.Services.Notification.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Notification;

public class NotificationEventOrdersController : ApiControllerBase
{
    private readonly INotificationEventOrdersHttpClient _httpClient;

    public NotificationEventOrdersController(INotificationEventOrdersHttpClient httpClient)
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
    public async Task<PaginatedList<NotificationEventOrderDto>> GetAllAsync([FromQuery] GetAllNotificationEventOrdersRequest request)
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
    public async Task<PaginatedList<NotificationEventOrderHistoryDto>> GetNotificationOrderHistoriesByOrderId([FromRoute] Guid orderId)
    {
        return await _httpClient.GetNotificationOrderHistoriesByOrderId(orderId);
    }
}
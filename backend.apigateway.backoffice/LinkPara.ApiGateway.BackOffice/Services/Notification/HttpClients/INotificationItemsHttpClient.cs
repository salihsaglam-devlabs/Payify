using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Services.Notification.HttpClients;

public interface INotificationItemsHttpClient
{
    Task CreateAsync(CreateNotificationItemRequest request);
    Task UpdateAsync(UpdateNotificationItemRequest request);
    Task<PaginatedList<NotificationItemDto>> GetAllAsync([FromQuery] GetAllNotificationItemsRequest request);
    Task<NotificationItemDto> GetNotificationItemById([FromQuery] Guid id);
    Task DeleteNotificationItemById(Guid id);
}
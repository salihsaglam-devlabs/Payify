using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Notification.HttpClients;

public interface INotificationEventItemsHttpClient
{
    Task CreateAsync(CreateNotificationEventItemRequest request);
    Task UpdateAsync(UpdateNotificationEventItemRequest request);
    Task<PaginatedList<NotificationEventItemDto>> GetAllAsync(GetAllNotificationEventItemRequest request);
    Task DeleteTemplateById(Guid eventItemId);
    Task<NotificationEventItemDto> GetById(Guid eventItemId);
}
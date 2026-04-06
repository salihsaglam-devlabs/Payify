using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Notification.HttpClients;

public interface INotificationEventOrdersHttpClient
{
    Task<PaginatedList<NotificationEventOrderDto>> GetAllAsync(GetAllNotificationEventOrdersRequest request);
    Task<PaginatedList<NotificationEventOrderHistoryDto>> GetNotificationOrderHistoriesByOrderId(Guid orderId);
}
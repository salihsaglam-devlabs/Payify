using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Notification.HttpClients;

public interface INotificationOrdersHttpClient
{
    Task<PaginatedList<NotificationOrderDto>> GetAllAsync(GetAllNotificationOrdersRequest request);
    Task<PaginatedList<NotificationOrderHistoryDto>> GetNotificationOrderHistoriesByOrderId(Guid orderId);
}
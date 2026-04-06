using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Notification.HttpClients;

public class NotificationEventOrdersHttpClient : HttpClientBase, INotificationEventOrdersHttpClient
{
    public NotificationEventOrdersHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) 
        : base(client, httpContextAccessor)
    {
    }

    public async Task<PaginatedList<NotificationEventOrderDto>> GetAllAsync(GetAllNotificationEventOrdersRequest request)
    {
        var url = CreateUrlWithParams($"v1/NotificationEventOrders", request, true);

        var response = await GetAsync(url);

        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();

        var notificationOrderList = await response.Content.ReadFromJsonAsync<PaginatedList<NotificationEventOrderDto>>();

        return notificationOrderList ?? throw new InvalidOperationException();
    }

    public async Task<PaginatedList<NotificationEventOrderHistoryDto>> GetNotificationOrderHistoriesByOrderId(Guid orderId)
    {
        var response = await GetAsync($"v1/NotificationEventOrders/history/{orderId}");
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();

        var historyList = await response.Content.ReadFromJsonAsync<PaginatedList<NotificationEventOrderHistoryDto>>();

        return historyList ?? throw new InvalidOperationException();
    }
}
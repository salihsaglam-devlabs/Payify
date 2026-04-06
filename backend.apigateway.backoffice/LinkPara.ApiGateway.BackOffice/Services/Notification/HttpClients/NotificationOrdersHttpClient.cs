using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Notification.HttpClients;

public class NotificationOrdersHttpClient : HttpClientBase, INotificationOrdersHttpClient
{
    public NotificationOrdersHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) 
        : base(client, httpContextAccessor)
    {
    }

    public async Task<PaginatedList<NotificationOrderDto>> GetAllAsync(GetAllNotificationOrdersRequest request)
    {
        var url = CreateUrlWithParams($"v1/NotificationOrders", request, true);

        var response = await GetAsync(url);

        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();

        var notificationOrderList = await response.Content.ReadFromJsonAsync<PaginatedList<NotificationOrderDto>>();

        return notificationOrderList ?? throw new InvalidOperationException();
    }

    public async Task<PaginatedList<NotificationOrderHistoryDto>> GetNotificationOrderHistoriesByOrderId(Guid orderId)
    {
        var response = await GetAsync($"v1/NotificationOrders/history/{orderId}");
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();

        var historyList = await response.Content.ReadFromJsonAsync<PaginatedList<NotificationOrderHistoryDto>>();

        return historyList ?? throw new InvalidOperationException();
    }
}
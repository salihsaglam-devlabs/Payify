using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Services.Notification.HttpClients;

public class NotificationItemsHttpClient : HttpClientBase, INotificationItemsHttpClient
{
    public NotificationItemsHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }
    public async Task CreateAsync(CreateNotificationItemRequest request)
    {
        var response = await PostAsJsonAsync($"v1/NotificationItems", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task UpdateAsync(UpdateNotificationItemRequest request)
    {
        var response = await PutAsJsonAsync($"v1/NotificationItems", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task<PaginatedList<NotificationItemDto>> GetAllAsync(GetAllNotificationItemsRequest request)
    {
        var url = CreateUrlWithParams($"v1/NotificationItems", request, true);

        var response = await GetAsync(url);

        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();

        var notificationItemList = await response.Content.ReadFromJsonAsync<PaginatedList<NotificationItemDto>>();

        return notificationItemList ?? throw new InvalidOperationException();
    }

    public async Task<NotificationItemDto> GetNotificationItemById(Guid id)
    {
        var response = await GetAsync($"v1/NotificationItems/{id}");

        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();

        var notificationTemplateList = await response.Content.ReadFromJsonAsync<NotificationItemDto>();

        return notificationTemplateList ?? throw new InvalidOperationException();
    }

    public async Task DeleteNotificationItemById(Guid id)
    {
        var response = await DeleteAsync($"v1/NotificationItems/{id}");
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }
}
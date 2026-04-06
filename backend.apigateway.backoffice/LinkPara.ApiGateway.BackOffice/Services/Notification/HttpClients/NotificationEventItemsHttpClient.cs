using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Notification.HttpClients;

public class NotificationEventItemsHttpClient : HttpClientBase, INotificationEventItemsHttpClient
{
    public NotificationEventItemsHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) 
        : base(client, httpContextAccessor)
    {
    }

    public async Task CreateAsync(CreateNotificationEventItemRequest request)
    {
        var response = await PostAsJsonAsync($"v1/NotificationEventItems", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task UpdateAsync(UpdateNotificationEventItemRequest request)
    {
        var response = await PutAsJsonAsync($"v1/NotificationEventItems", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task<PaginatedList<NotificationEventItemDto>> GetAllAsync(GetAllNotificationEventItemRequest request)
    {
        var url = CreateUrlWithParams($"v1/NotificationEventItems", request, true);

        var response = await GetAsync(url);

        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();

        var notificationEventItemList = await response.Content.ReadFromJsonAsync<PaginatedList<NotificationEventItemDto>>();

        return notificationEventItemList ?? throw new InvalidOperationException();
    }

    public async Task DeleteTemplateById(Guid eventItemId)
    {
        var response = await DeleteAsync($"v1/NotificationEventItems/{eventItemId}");
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task<NotificationEventItemDto> GetById(Guid eventItemId)
    {
        var response = await GetAsync($"v1/NotificationEventItems/{eventItemId}");

        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();

        var notificationEventItem = await response.Content.ReadFromJsonAsync<NotificationEventItemDto>();

        return notificationEventItem ?? throw new InvalidOperationException();
    }
}
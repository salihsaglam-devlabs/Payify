using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Notification.HttpClients;

public class NotificationTemplatesHttpClient : HttpClientBase, INotificationTemplatesHttpClient
{
    public NotificationTemplatesHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
    : base(client, httpContextAccessor)
    {
    }

    public async Task CreateNotificationTemplate(CreateNotificationTemplateRequest request)
    {
        var response = await PostAsJsonAsync($"v1/NotificationTemplates", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task DeleteNotificationTemplate(Guid templateId)
    {
        var response = await DeleteAsync($"v1/NotificationTemplates/{templateId}");
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task<PaginatedList<NotificationTemplateDto>> GetAllNotificationTemplates(TemplateFilterQuery request)
    {
        var url = CreateUrlWithParams($"v1/NotificationTemplates", request, true);

        var response = await GetAsync(url);

        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();

        var notificationTemplateList = await response.Content.ReadFromJsonAsync<PaginatedList<NotificationTemplateDto>>();

        return notificationTemplateList ?? throw new InvalidOperationException();
    }

    public async Task<NotificationTemplateDto> GetNotificationTemplateById(Guid templateId)
    {
        var response = await GetAsync($"v1/NotificationTemplates/{templateId}");

        var template = await response.Content.ReadFromJsonAsync<NotificationTemplateDto>();

        return template ?? throw new InvalidOperationException();
    }

    public async Task UpdateNotificationTemplate(UpdateNotificationTemplateRequest request)
    {
        var response = await PutAsJsonAsync($"v1/NotificationTemplates", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }
}

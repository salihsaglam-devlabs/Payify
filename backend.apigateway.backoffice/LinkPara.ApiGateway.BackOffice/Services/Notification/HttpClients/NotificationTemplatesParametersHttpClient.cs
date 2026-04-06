using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Enums;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Services.Notification.HttpClients;

public class NotificationTemplatesParametersHttpClient : HttpClientBase, INotificationTemplateParametersHttpClient
{
    public NotificationTemplatesParametersHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }
    public async Task<List<string>> GetAllParametersWithDisplayNames(ContentLanguage language)
    {
        var response = await GetAsync($"v1/NotificationTemplateParameters?language={language}");

        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();

        var notificationTemplateParametersList = await response.Content.ReadFromJsonAsync<List<string>>();

        return notificationTemplateParametersList ?? throw new InvalidOperationException();
    }
}
using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Enums;
using LinkPara.SharedModels.Notification;

namespace LinkPara.ApiGateway.BackOffice.Services.Notification.HttpClients;

public class NotificationEventsHttpClient : HttpClientBase, INotificationEventsHttpClient
{
    public NotificationEventsHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task<Dictionary<string, string>> GetAllEventNamesAsync(EventNotificationField eventNotificationField, ContentLanguage language)
    {
        var url = $"v1/NotificationEvents?eventNotificationField={eventNotificationField}&language={language}";

        var response = await GetAsync(url);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException("Failed to get notification event names.");

        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        return result ?? throw new InvalidOperationException("Result is null.");
    }

    public async Task<List<string>> GetEventParametersAsync(string eventName, ContentLanguage language)
    {
        var url = $"v1/NotificationEvents/eventName?eventName={eventName}&language={language}";

        var response = await GetAsync(url);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException("Failed to get event parameters.");

        var result = await response.Content.ReadFromJsonAsync<List<string>>();
        return result ?? throw new InvalidOperationException("Result is null.");
    }
}
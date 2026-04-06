using LinkPara.HttpProviders.Identity.Models;
using LinkPara.HttpProviders.Notification.Models;
using LinkPara.HttpProviders.Utility;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Web;

namespace LinkPara.HttpProviders.Notification;

public class NotificationService : HttpClientBase, INotificationService
{
    public NotificationService(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {

    }

    public async Task<NotificationResponse> SendPushNotificationAsync(NotificationRequest request)
    {
        var response = await PostAsJsonAsync($"v1/PushNotification",request);

        if (response.IsSuccessStatusCode)
        {
            var responseString = await response.Content.ReadAsStringAsync();

            var user = JsonSerializer.Deserialize<NotificationResponse>(responseString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return user;
        }
        throw new InvalidOperationException();
    }
    
    public async Task<NotificationResponse> SendSmsNotificationAsync(SmsRequest request)
    {
        var response = await PostAsJsonAsync($"v1/Sms",request);

        if (response.IsSuccessStatusCode)
        {
            var responseString = await response.Content.ReadAsStringAsync();

            var user = JsonSerializer.Deserialize<NotificationResponse>(responseString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return user;
        }
        throw new InvalidOperationException();
    }
    
    public async Task<NotificationResponse> SendEmailNotificationAsync(EmailRequest request)
    {
        var response = await PostAsJsonAsync($"v1/Emails",request);

        if (response.IsSuccessStatusCode)
        {
            var responseString = await response.Content.ReadAsStringAsync();

            var user = JsonSerializer.Deserialize<NotificationResponse>(responseString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return user;
        }
        throw new InvalidOperationException();
    }
}
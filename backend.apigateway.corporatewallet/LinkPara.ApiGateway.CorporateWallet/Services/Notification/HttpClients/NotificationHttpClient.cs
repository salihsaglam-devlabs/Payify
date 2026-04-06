using LinkPara.HttpProviders.Notification.Models;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Notification.HttpClients;

public class NotificationHttpClient : HttpClientBase, INotificationHttpClient
{
    
    public NotificationHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }
    
    public async Task<NotificationResponse> SendSmsNotificationAsync(SmsRequest request)
    {
        var response = await PostAsJsonAsync($"v1/Sms",request);

        return await response.Content.ReadFromJsonAsync<NotificationResponse>();
    }
    
    public async Task<NotificationResponse> SendEmailNotificationAsync(EmailRequest request)
    {
        var response = await PostAsJsonAsync($"v1/Emails",request);

        return await response.Content.ReadFromJsonAsync<NotificationResponse>();
    }
}
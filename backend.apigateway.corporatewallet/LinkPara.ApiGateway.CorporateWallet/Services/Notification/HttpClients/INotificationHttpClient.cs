using LinkPara.HttpProviders.Notification.Models;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Notification.HttpClients;

public interface INotificationHttpClient
{
    Task<NotificationResponse> SendSmsNotificationAsync(SmsRequest request);
    Task<NotificationResponse> SendEmailNotificationAsync(EmailRequest request);
}
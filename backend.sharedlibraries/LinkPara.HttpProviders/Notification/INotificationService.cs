using LinkPara.HttpProviders.Identity.Models;
using LinkPara.HttpProviders.Notification.Models;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.HttpProviders.Notification;

public interface INotificationService
{
    Task<NotificationResponse> SendPushNotificationAsync(NotificationRequest request);
    Task<NotificationResponse> SendSmsNotificationAsync(SmsRequest request);
    Task<NotificationResponse> SendEmailNotificationAsync(EmailRequest request);
}
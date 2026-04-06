using LinkPara.SharedModels.BusModels.Commands.Notification;

namespace LinkPara.Identity.Application.Common.Interfaces;

public interface IPushNotificationSender
{
    Task SendPushNotificationAsync(SendPushNotification request);
}

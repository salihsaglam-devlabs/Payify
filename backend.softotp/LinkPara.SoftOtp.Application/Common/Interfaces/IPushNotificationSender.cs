using LinkPara.SharedModels.BusModels.Commands.Notification;

namespace LinkPara.SoftOtp.Application.Common.Interfaces;

public interface IPushNotificationSender
{
    Task SendPushNotificationAsync(SendPushNotification request);
}
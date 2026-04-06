namespace LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Enums;

[Flags]
public enum NotificationMethod
{
    None = 0,
    Email = 1,
    SMS = 2,
    PortalInbox = 4,
    Statement = 8,
    PushNotification = 16
}
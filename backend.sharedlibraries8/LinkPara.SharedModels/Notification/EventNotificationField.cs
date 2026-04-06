namespace LinkPara.SharedModels.Notification;

[Flags]
public enum EventNotificationField
{
    All = 0,
    Backoffice = 1,
    PF = 2,
    Emoney = 4
}
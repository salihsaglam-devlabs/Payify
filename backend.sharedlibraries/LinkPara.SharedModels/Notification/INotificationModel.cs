namespace LinkPara.SharedModels.Notification;

public interface INotificationModel
{
    string NotificationName { get; }
    string DisplayName { get; }
    Dictionary<string, string> Parameters { get; }
    List<Dictionary<string, string>> AttachmentList { get; }
}
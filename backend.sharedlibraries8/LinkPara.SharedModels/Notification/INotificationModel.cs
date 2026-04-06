namespace LinkPara.SharedModels.Notification;

public interface INotificationModel
{
    string NotificationName { get; }
    string DisplayName { get; }
    Dictionary<string, string> Parameters { get; }
    List<Dictionary<string, string>> AttachmentList { get; }
    public Dictionary<string, string> ParametersWithFieldName { get; }
    public EventNotificationField EventNotificationField { get; set; }
}
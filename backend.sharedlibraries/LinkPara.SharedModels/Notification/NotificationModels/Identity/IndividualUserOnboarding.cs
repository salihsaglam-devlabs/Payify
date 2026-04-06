namespace LinkPara.SharedModels.Notification.NotificationModels.Identity;

public class IndividualUserOnboarding : NotificationBase, INotificationOrder
{
    public string Link { get; set; }
    public string Username { get; set; }
    public Guid UserId { get; set; }
}
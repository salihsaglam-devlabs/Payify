namespace LinkPara.SharedModels.Notification;

public interface INotificationOrder : INotificationModel
{
    Guid UserId { get; set; }
}
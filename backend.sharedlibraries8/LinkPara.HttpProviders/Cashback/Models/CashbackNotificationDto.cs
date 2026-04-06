using LinkPara.SharedModels.BusModels.Commands.Notification;

namespace LinkPara.HttpProviders.Cashback.Models;

public class CashbackNotificationDto
{
    public List<string> RegistrationTokens { get; set; }
    public List<NotificationUserInfo> UserList { get; set; }
}

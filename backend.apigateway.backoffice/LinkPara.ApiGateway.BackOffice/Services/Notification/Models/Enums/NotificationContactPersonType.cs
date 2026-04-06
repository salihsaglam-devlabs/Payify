namespace LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Enums;

[Flags]
public enum NotificationContactPersonType
{
    None = 0,
    AuthorizedPerson = 1,
    TechnicalPerson = 2,
    AuthorizedUsers = 4,
}
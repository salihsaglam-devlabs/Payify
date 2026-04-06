using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Enums;
using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Requests;
using LinkPara.SharedModels.Notification;

namespace LinkPara.ApiGateway.BackOffice.Services.Notification.HttpClients;

public interface INotificationEventsHttpClient
{
    Task<Dictionary<string, string>> GetAllEventNamesAsync(EventNotificationField eventNotificationField, ContentLanguage language);
    Task<List<string>> GetEventParametersAsync(string eventName, ContentLanguage language);
}
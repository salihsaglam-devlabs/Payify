using LinkPara.SharedModels.Notification.NotificationModels.Identity;

namespace LinkPara.Identity.Application.Common.Interfaces;

public interface INotificationTemplateParametersService
{
    List<string> GetAllNotificationTemplateParameterNamesAsync(string language);
    Task<IdentityCustomNotificationParameters> GetNotificationParameterTemplateValuesAsync(Guid userId, string language);
}
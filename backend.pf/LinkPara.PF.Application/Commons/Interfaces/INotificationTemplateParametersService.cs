using LinkPara.SharedModels.Notification.NotificationModels.PF;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface INotificationTemplateParametersService
{
    List<string> GetAllNotificationTemplateParameterNamesAsync(string language);
    Task<PfCustomNotificationParameters> GetNotificationParameterTemplateValuesAsync(Guid merchantId, string language);
}
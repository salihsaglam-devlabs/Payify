using LinkPara.HttpProviders.PF.Models.Request;
using LinkPara.SharedModels.Notification.NotificationModels.PF;

namespace LinkPara.HttpProviders.PF;

public interface IPfParametersService
{
    Task<List<string>> GetNotificationParametersWithDisplayNamesAsync(string language);

    Task<PfCustomNotificationParameters> GetNotificationParameterValuesAsync(GetPfParameterValuesRequest request);
}
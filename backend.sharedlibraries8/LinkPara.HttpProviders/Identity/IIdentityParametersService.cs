using LinkPara.HttpProviders.Identity.Models;
using LinkPara.SharedModels.Notification.NotificationModels.Identity;

namespace LinkPara.HttpProviders.Identity;

public interface IIdentityParametersService
{
    Task<List<string>> GetNotificationParametersWithDisplayNamesAsync(string language);

    Task<IdentityCustomNotificationParameters> GetNotificationParameterValuesAsync(GetIdentityParametersRequest request);
}
using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Enums;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Services.Notification.HttpClients;

public interface INotificationTemplateParametersHttpClient
{
    public Task<List<string>> GetAllParametersWithDisplayNames(ContentLanguage language);
}
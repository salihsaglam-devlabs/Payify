using DocumentFormat.OpenXml.Office2010.ExcelAc;
using LinkPara.ApiGateway.BackOffice.Services.Notification.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Notification;

public class NotificationTemplateCustomParameters : ApiControllerBase
{
    private readonly INotificationTemplateParametersHttpClient _httpClient;

    public NotificationTemplateCustomParameters(INotificationTemplateParametersHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Returns all Notification template parameters display names.
    /// </summary>
    /// <param name="language"></param>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize]
    public async Task<List<string>> GetCustomParameters([FromQuery] ContentLanguage language)
    {
        return await _httpClient.GetAllParametersWithDisplayNames(language);
    }
}
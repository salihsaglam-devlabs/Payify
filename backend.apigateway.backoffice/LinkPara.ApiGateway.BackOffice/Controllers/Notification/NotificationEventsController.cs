using LinkPara.ApiGateway.BackOffice.Services.Notification.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Enums;
using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Requests;
using LinkPara.SharedModels.Notification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Notification;

public class NotificationEventsController : ApiControllerBase
{
    private readonly INotificationEventsHttpClient _httpClient;

    public NotificationEventsController(INotificationEventsHttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    /// <summary>
    /// Returns all Notification templates.
    /// </summary>
    /// <param name="language"></param>
    /// <param name="eventNotificationField"></param>
    /// <returns></returns>
    [HttpGet("events/names")]
    [Authorize]
    public async Task<ActionResult<Dictionary<string, string>>> GetAllEventNamesAsync([FromQuery] EventNotificationField eventNotificationField, [FromQuery]ContentLanguage language)
    {
        return await _httpClient.GetAllEventNamesAsync(eventNotificationField, language);
    }
    
    /// <summary>
    /// Returns all Notification templates.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("events/parameters")]
    [Authorize]
    public async Task<ActionResult<List<string>>> GetAllParametersAsync([FromQuery]GetTemplateEventsRequest request)
    {
        return await _httpClient.GetEventParametersAsync(request.EventName, request.Language);
    }
}
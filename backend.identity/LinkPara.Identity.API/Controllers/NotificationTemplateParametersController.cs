using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.SharedModels.Notification.NotificationModels.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Identity.API.Controllers;

public class NotificationTemplateParametersController : ApiControllerBase
{
    private readonly INotificationTemplateParametersService _notificationTemplateParametersService;
    public NotificationTemplateParametersController(INotificationTemplateParametersService notificationTemplateParametersService)
    {
        _notificationTemplateParametersService = notificationTemplateParametersService;
    }
    
    /// <summary>
    /// Returns parameter list that can be used in notification templates
    /// </summary>
    /// <returns></returns>
    //[Authorize]
    [HttpGet("{language}")]
    public ActionResult<List<string>> GetAllTemplateParameterNamesAsync([FromRoute] string language)
    {
        return _notificationTemplateParametersService.GetAllNotificationTemplateParameterNamesAsync(language);
    }
    
    /// <summary>
    /// Returns parameter values list
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="language"></param>
    /// <returns></returns>
    //[Authorize]
    [HttpGet("values/{language}/{userId:guid}")]
    public async Task<ActionResult<IdentityCustomNotificationParameters>> GetAllTemplateParameterValuesAsync([FromRoute] Guid userId, [FromRoute] string language)
    {
        return await _notificationTemplateParametersService.GetNotificationParameterTemplateValuesAsync(userId, language);
    }
}
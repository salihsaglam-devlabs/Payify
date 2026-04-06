using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.SharedModels.Notification.NotificationModels.PF;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.PF.API.Controllers;

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
    /// <param name="language"></param>
    /// <returns></returns>
    [Authorize]
    [HttpGet("{language}")]
    public ActionResult<List<string>> GetAllTemplateParameterNamesAsync([FromRoute] string language)
    {
        return _notificationTemplateParametersService.GetAllNotificationTemplateParameterNamesAsync(language);
    }
    
    /// <summary>
    /// Returns parameter values list
    /// </summary>
    /// <param name="merchantId"></param>
    /// <param name="language"></param>
    /// <returns></returns>
    [Authorize]
    [HttpGet("values/{language}/{merchantId}")]
    public async Task<ActionResult<PfCustomNotificationParameters>> GetAllTemplateParameterValuesAsync([FromRoute] Guid merchantId, [FromRoute] string language)
    {
        return await _notificationTemplateParametersService.GetNotificationParameterTemplateValuesAsync(merchantId,  language);
    }
}
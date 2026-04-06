using LinkPara.PF.Application.Features.MerchantNotification;
using LinkPara.PF.Application.Features.MerchantNotification.Queries.GetMerchantNotificationTemplate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.PF.API.Controllers;

public class MerchantNotificationController : ApiControllerBase
{
    /// <summary>
    /// Returns merchant notification template.
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantNotification:Read")]
    [HttpGet]
    public async Task<MerchantNotificationTemplateDto> GetMerchantNotificationTemplateAsync([FromQuery] GetMerchantNotificationTemplateQuery query)
    {
        return await Mediator.Send(query);
    }
}
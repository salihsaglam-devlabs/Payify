
using LinkPara.Emoney.Application.Features.CallCenterCustomerAccount;
using LinkPara.Emoney.Application.Features.CallCenterCustomerAccount.Commands.CallCenterCustomerConfirmation;
using LinkPara.Emoney.Application.Features.CallCenterCustomerAccount.Commands.SendCallCenterNotificationAction;
using LinkPara.Emoney.Application.Features.CallCenterCustomerAccount.Queries.GetCallCenterNotificationStatus;
using LinkPara.Emoney.Application.Features.CallCenterCustomerAccount.Queries.GetCustomerAccountInfo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Emoney.API.Controllers;

public class CallCenterCustomerAccountController : ApiControllerBase
{
    /// <summary>
    /// This method used to confirm call center customers. It also send notification to customers' phone.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "CallCenter:Create")]
    [HttpPost("confirm")]
    public async Task<CustomerConfirmationResponse> CustomerConfirmationAsync([FromBody] CallCenterCustomerConfirmationCommand command)
    {
        return await Mediator.Send(command);
    }

    /// <summary>
    /// This method used to get account informations of confirmed customer.
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "CallCenter:Read")]
    [HttpGet("account-info")]
    public async Task<ActionResult<CustomerAccountInfoResponse>> GetCustomerAccountInfoAsync([FromQuery] GetCustomerAccountInfoQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// This method used to get status of push notification.
    /// </summary>
    /// <param name="notificationId"></param>
    /// <returns></returns>
    [Authorize(Policy = "CallCenter:Read")]
    [HttpGet("notification-status/{notificationId}")]
    public async Task<ActionResult<CallCenterNotificationStatusResponse>> GetCallCenterNotificationStatusAsync([FromRoute] Guid notificationId)
    {
        return await Mediator.Send(new GetCallCenterNotificationStatusQuery { NotificationId = notificationId });
    }

    /// <summary>
    /// This method used to send push notification action. approved or reject.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "CallCenterNotification:Update")]
    [HttpPut("notification-action")]
    public async Task<CallCenterNotificationActionResponse> SendCallCenterNotificationActionAsync([FromBody] SendCallCenterNotificationActionCommand command)
    {
        return await Mediator.Send(command);
    }
}


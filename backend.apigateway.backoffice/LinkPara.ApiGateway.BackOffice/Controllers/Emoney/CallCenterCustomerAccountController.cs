using LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Emoney;

public class CallCenterCustomerAccountController : ApiControllerBase
{
    private readonly ICallCenterCustomerAccountHttpClient _callCenterCustomerAccountHttpClient;

    public CallCenterCustomerAccountController(ICallCenterCustomerAccountHttpClient callCenterCustomerAccountHttpClient)
    {
        _callCenterCustomerAccountHttpClient = callCenterCustomerAccountHttpClient;
    }

    /// <summary>
    /// This method used to confirm call center customers. It also send notification to customers' phone.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "CallCenter:Create")]
    [HttpPost("confirm")]
    public async Task<ActionResult<CustomerConfirmationResponse>> CustomerConfirmationAsync([FromBody] CustomerConfirmationRequest request)
    {
        return await _callCenterCustomerAccountHttpClient.CustomerConfirmationAsync(request);
    }

    /// <summary>
    /// This method used to get account informations of confirmed customer.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "CallCenter:Read")]
    [HttpGet("account-info")]
    public async Task<ActionResult<CustomerAccountInfoResponse>> GetCustomerAccountInfoAsync([FromQuery] GetCustomerAccountInfoRequest request)
    {
        return await _callCenterCustomerAccountHttpClient.GetCustomerAccountInfoAsync(request);
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
        return await _callCenterCustomerAccountHttpClient.GetCallCenterNotificationStatusAsync(notificationId);
    }
}

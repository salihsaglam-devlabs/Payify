
using LinkPara.ApiGateway.Services.Emoney.HttpClients;
using LinkPara.ApiGateway.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Services.Emoney.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Controllers.Emoney;

public class CallCenterCustomerAccountController : ApiControllerBase
{
    private readonly ICallCenterCustomerAccountHttpClient _callCenterCustomerAccountHttpClient;

    public CallCenterCustomerAccountController(ICallCenterCustomerAccountHttpClient callCenterCustomerAccountHttpClient)
    {
        _callCenterCustomerAccountHttpClient = callCenterCustomerAccountHttpClient;
    }

    /// <summary>
    /// This method used to send push notification action. approved or reject.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "CallCenterNotification:Update")]
    [HttpPut("notification-action")]
    public async Task<CallCenterNotificationActionResponse> SendCallCenterNotificationActionAsync([FromBody] SendCallCenterNotificationActionRequest request)
    {
        return await _callCenterCustomerAccountHttpClient.SendCallCenterNotificationActionAsync(request);
    }

}

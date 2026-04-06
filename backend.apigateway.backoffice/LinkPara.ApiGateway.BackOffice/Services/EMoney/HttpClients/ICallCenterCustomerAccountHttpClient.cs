using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;

public interface ICallCenterCustomerAccountHttpClient
{
    Task<CustomerConfirmationResponse> CustomerConfirmationAsync(CustomerConfirmationRequest request);
    Task<CustomerAccountInfoResponse> GetCustomerAccountInfoAsync(GetCustomerAccountInfoRequest request);
    Task<CallCenterNotificationStatusResponse> GetCallCenterNotificationStatusAsync(Guid notificationId);
}

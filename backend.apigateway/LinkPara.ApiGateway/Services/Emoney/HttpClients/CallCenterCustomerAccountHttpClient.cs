using LinkPara.ApiGateway.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Services.Emoney.Models.Responses;

namespace LinkPara.ApiGateway.Services.Emoney.HttpClients;

public class CallCenterCustomerAccountHttpClient : HttpClientBase, ICallCenterCustomerAccountHttpClient
{
    public CallCenterCustomerAccountHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task<CallCenterNotificationActionResponse> SendCallCenterNotificationActionAsync(SendCallCenterNotificationActionRequest request)
    {
        var response = await PutAsJsonAsync($"v1/CallCenterCustomerAccount/notification-action", request);
        var sendActionResponse = await response.Content.ReadFromJsonAsync<CallCenterNotificationActionResponse>();

        return sendActionResponse ?? throw new InvalidOperationException();
    }
}
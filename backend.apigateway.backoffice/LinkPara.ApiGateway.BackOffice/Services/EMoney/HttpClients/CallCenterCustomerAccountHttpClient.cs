using LinkPara.ApiGateway.BackOffice.Commons.Helpers;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;
using LinkPara.ApiGateway.BackOffice.Services.Notification.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Requests;
using LinkPara.SharedModels.Pagination;
using Microsoft.Identity.Client;
using System.Text.Json;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;

public class CallCenterCustomerAccountHttpClient : HttpClientBase, ICallCenterCustomerAccountHttpClient
{
    private readonly IUserSimBlockageHttpClient _simBlockageHttpClient;
    public CallCenterCustomerAccountHttpClient(
        HttpClient client,
        IHttpContextAccessor httpContextAccessor,
        IUserSimBlockageHttpClient simBlockageHttpClient)
        : base(client, httpContextAccessor)
    {
        _simBlockageHttpClient = simBlockageHttpClient;
    }

    public async Task<CustomerConfirmationResponse> CustomerConfirmationAsync(CustomerConfirmationRequest request)
    {
        var response = await PostAsJsonAsync($"v1/CallCenterCustomerAccount/confirm", request);
        var responseString = await response.Content.ReadAsStringAsync();
        var custConfirmResponse = JsonSerializer.Deserialize<CustomerConfirmationResponse>(responseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return custConfirmResponse;
    }
    public async Task<CustomerAccountInfoResponse> GetCustomerAccountInfoAsync(GetCustomerAccountInfoRequest request)
    {
        var url = CreateUrlWithParams($"v1/CallCenterCustomerAccount/account-info", request, true);
        var response = await GetAsync(url);
        var accountInfoResponse = await response.Content.ReadFromJsonAsync<CustomerAccountInfoResponse>();
        if (accountInfoResponse != null && accountInfoResponse.AccountInformation != null)
        {
            var accountInfo = accountInfoResponse.AccountInformation;
            var phoneCode = !String.IsNullOrEmpty(accountInfo.PhoneCode) && accountInfo.PhoneCode.StartsWith("+") ? accountInfo.PhoneCode.Substring(1) : accountInfo.PhoneCode;
            var simBlockageRequest = new GetUserSimBlockageRequest() { PhoneNumber = $"{phoneCode}{accountInfo.PhoneNumber}", SortBy = "createDate", OrderBy = OrderByStatus.Desc };
            var simBlockageResult = await _simBlockageHttpClient.GetUserSimBlockageListAsync(simBlockageRequest);
            var simBlockageObj = simBlockageResult != null && simBlockageResult.Items.Count > 0 ? simBlockageResult.Items.FirstOrDefault() : null;

            accountInfoResponse.AccountInformation.IsSimBlockage = simBlockageObj != null && !simBlockageObj.IsSendOtp;
        }

        return accountInfoResponse ?? throw new InvalidOperationException();
    }

    public async Task<CallCenterNotificationStatusResponse> GetCallCenterNotificationStatusAsync(Guid notificationId)
    {
        var response = await GetAsync($"v1/CallCenterCustomerAccount/notification-status/{notificationId}");
        var statusResponse = await response.Content.ReadFromJsonAsync<CallCenterNotificationStatusResponse>();
        return statusResponse ?? throw new InvalidOperationException();
    }

}

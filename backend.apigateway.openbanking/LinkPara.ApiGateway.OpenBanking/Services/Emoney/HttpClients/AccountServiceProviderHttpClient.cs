
using LinkPara.ApiGateway.OpenBanking.Commons.Extensions;
using LinkPara.ApiGateway.OpenBanking.Services.Emoney.Requests;
using LinkPara.ApiGateway.OpenBanking.Services.Emoney.Responses;
using System.Text.Json;
using static LinkPara.ApiGateway.OpenBanking.Services.Emoney.Responses.UserAccountResultDto;
using static MassTransit.ValidationResultExtensions;

namespace LinkPara.ApiGateway.OpenBanking.Services.Emoney.HttpClients;

public class AccountServiceProviderHttpClient : HttpClientBase, IAccountServiceProviderHttpClient
{
    public AccountServiceProviderHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task<BaseServiceResponse<CustomerConfirmationResponse>> CustomerConfirmationAsync(CustomerConfirmationRequest request)
    {
        try
        {
            var result = await PostAsJsonAsync($"v1/AccountServiceProviders/customer-confirmation", request);
            var response = await result.Content.ReadFromJsonAsync<CustomerConfirmationResponse>();
            return new BaseServiceResponse<CustomerConfirmationResponse>
            {
                Value = response,
                Success = result.IsSuccessStatusCode,
            };
        }
        catch (Exception)
        {
            return new BaseServiceResponse<CustomerConfirmationResponse>
            {
                Success = false,
            };
        }
        
    }

    public async Task<AccountTransactionsDto> GetAccountTransactionsAsync(GetAccountTransactionsRequest request)
    {
        var result = await PostAsJsonAsync($"v1/AccountServiceProviders/account-transaction/{AccountId}", request);
        var response = await result.Content.ReadFromJsonAsync<AccountTransactionsDto>();
        return response;
    }

    public async Task<List<ChangedBalanceDto>> GetChangedBalanceAsync()
    {
        var result = await GetAsync($"v1/AccountServiceProviders/changed-balances");
        var response = await result.Content.ReadFromJsonAsync<List<ChangedBalanceDto>>();
        return response;
    }

    public async Task<SendNotificationResultDto> SendGkdNotificationAsync(SendGkdNotificationRequest request)
    {
        var result = await PostAsJsonAsync($"v1/AccountServiceProviders/gkd-notification/{AccountId}", request);
        var response = await result.Content.ReadFromJsonAsync<SendNotificationResultDto>();
        return response;
    }

    public async Task<SendOtpMessageResultDto> SendOtpMessageAsync(SendOtpMessageRequest request)
    {
        var result = await PostAsJsonAsync($"v1/AccountServiceProviders/otp-message/{AccountId}", request);
        var response = await result.Content.ReadFromJsonAsync<SendOtpMessageResultDto>();
        return response;
    }

    public async Task<BaseServiceResponse<PaymentContractDto>> CreatePaymentOrderAsync(CreatePaymentOrderRequest request)
    {
        var result = await PostAsJsonAsync($"v1/AccountServiceProviders/create-payment-order", request);
        var response = await result.Content.ReadFromJsonAsync<BaseServiceResponse<PaymentContractDto>>();
        return response;
    }

    public async Task<BaseServiceResponse<PaymentContractDto>> PaymentOrderInquiryAsync(PaymentOrderInquiryRequest request)
    {
        var result = await PostAsJsonAsync($"v1/AccountServiceProviders/payment-order-inquiry/{AccountId}", request);
        var response = await result.Content.ReadFromJsonAsync<BaseServiceResponse<PaymentContractDto>>();
        return response;
    }

    public async Task<GetApprovalScreenWalletListResponse> GetApprovalScreenWalletsAsync(GetApprovalScreenWalletListRequest request)
    {
        var queryString = request.GetQueryString();
        var response = await GetAsync($"v1/AccountServiceProviders/approval-screen-wallet-list{queryString}");
        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<List<ApprovalScreenWalletDto>>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return new GetApprovalScreenWalletListResponse { Value = result };
    }

    public async Task<GetWalletListResponse> GetWalletsAsync(GetWalletInfoRequest request)
    {
        var queryString = request.GetQueryString();
        var response = await GetAsync($"v1/AccountServiceProviders/wallet-list/{AccountId}" + queryString);
        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<List<CustomerWaletDto>>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return new GetWalletListResponse { RecordList = result, TotalRecord = result.Count };
    }

    public async Task<GetWalletBalanceListResponse> GetWalletBalancesAsync(GetWalletInfoRequest request)
    {
         var queryString = request.GetQueryString();
        var response = await GetAsync($"v1/AccountServiceProviders/wallet-balance-list/{AccountId}" + queryString);
        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<List<CustomerWalletBalanceDto>>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return new GetWalletBalanceListResponse { RecordList = result, TotalRecord = result.Count };
    }

    public async Task<IdentityInfoDto> GetUserIdentityInfoAsync(GetUserIdentityInfoRequest request)
    {
        var queryString = request.GetQueryString();
        var response = await GetAsync($"v1/AccountServiceProviders/user-identity-info{queryString}");
        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<IdentityInfoDto>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return result;
    }

    public async Task<List<AccountDetail>> GetUserAccountListAsync(GetUserAccountListRequest request)
    {
        var queryString = request.GetQueryString();
        var response = await GetAsync($"v1/AccountServiceProviders/user-accounts{queryString}");
        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<List<AccountDetail>>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return result;
    }
}

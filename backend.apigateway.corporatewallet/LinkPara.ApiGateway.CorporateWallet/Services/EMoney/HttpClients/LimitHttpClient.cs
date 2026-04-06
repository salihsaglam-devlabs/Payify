
using LinkPara.ApiGateway.CorporateWallet.Services;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.HttpClients;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Responses;

namespace LinkPara.ApiGateway.Services.Emoney.HttpClients;

public class LimitHttpClient : HttpClientBase, ILimitHttpClient
{
    public LimitHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }
    
    public async Task<UserLimitDto> GetUserLimitsAsync(GetUserLimitsQuery request)
    {
        var response = await GetAsync($"v1/Limits/user?UserId={request.UserId}&CurrencyCode={request.CurrencyCode}");
        var userLimits = await response.Content.ReadFromJsonAsync<UserLimitDto>();
        #region Temporary Addings For Mobile
        userLimits.CorporateWallet = new BalanceLimitDto
        {
            CurrentBalance = 0,
            MaxBalance = 0
        };
        userLimits.CorporateWalletTransfer = new UsageLimitDto
        {
            DailyMaxAmount = 0,
            DailyMaxCount = 0,
            MonthlyMaxAmount = 0,
            MonthlyMaxCount = 0,
            DailyUserAmount = 0,
            DailyUserCount = 0,
            MonthlyUserAmount = 0,
            MonthlyUserCount = 0
        };
        #endregion
        return userLimits ?? throw new InvalidOperationException();
    }

    public async Task<AccountLimitDto> GetAccountLimitsAsync(GetAccountLimitsQuery request)
    {
        var response = await GetAsync($"v1/Limits/account?AccountId={request.AccountId}&CurrencyCode={request.CurrencyCode}");
        var accountLimits = await response.Content.ReadFromJsonAsync<AccountLimitDto>();
        
        return accountLimits ?? throw new InvalidOperationException();
    }
}
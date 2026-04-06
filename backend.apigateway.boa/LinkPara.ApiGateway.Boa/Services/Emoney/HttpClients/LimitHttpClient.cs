using LinkPara.ApiGateway.Boa.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Boa.Services.Emoney.Models.Responses;

namespace LinkPara.ApiGateway.Boa.Services.Emoney.HttpClients;

public class LimitHttpClient : HttpClientBase, ILimitHttpClient
{
    public LimitHttpClient(HttpClient client, 
        IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
    {
    }


    public async Task<UserLimitDto> GetUserLimitsAsync(GetUserLimitsQuery request)
    {
        var response = await GetAsync($"v1/Limits/user?UserId={request.UserId}&CurrencyCode={request.CurrencyCode}");
        var userLimits = await response.Content.ReadFromJsonAsync<UserLimitDto>();

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

        return userLimits ?? throw new InvalidOperationException();
    }
}
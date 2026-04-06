using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
using LinkPara.ApiGateway.BackOffice.Commons.Extensions;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using System.Text;
using LinkPara.ApiGateway.BackOffice.Commons.Helpers;
using LinkPara.SharedModels.Pagination;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;

public class WalletHttpClient : HttpClientBase, IWalletHttpClient
{

    public WalletHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task ConvertUserWalletsToIndividualAsync(ConvertUserWalletsToIndividualRequest request)
    {
        await PostAsJsonAsync("v1/Wallets/convert-user-wallets-to-individual", request);
    }

    public async Task<List<WalletDto>> GetUserWalletsAsync(GetUserWalletsRequest request)
    {
        var queryString = request.GetQueryString();
        var response = await GetAsync($"v1/Wallets" + queryString);
        var responseString = await response.Content.ReadAsStringAsync();
        var walletList = JsonSerializer.Deserialize<List<WalletDto>>(responseString,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        return walletList ?? throw new InvalidOperationException();
    }

    public async Task UpdateWalletAsync(UpdateWalletRequest request)
    {
        var body = await new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json").ReadAsStringAsync();

        var response = await PatchAsJsonAsync($"v1/Wallets", body);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException();
    }

    public async Task UpdateUserWalletsAsync(Guid userId, UpdateUserWalletsRequest request)
    {
        request.UserId = userId;

        var response = await PutAsJsonAsync($"v1/Wallets/{userId}", request);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException();
    }

    public async Task<WalletSummeryDto> GetWalletSummaryAsync(GetWalletSummaryRequest request)
    {
        var url = CreateUrlWithProperties($"v1/Wallets/summary", request);
        var response = await GetAsync(url);

        var walletSummary = await response.Content.ReadFromJsonAsync<WalletSummeryDto>();

        return walletSummary ?? throw new InvalidOperationException();
    }

    public async Task<WalletBalanceResponse> GetWalletBalancesAsync(GetWalletBalanceRequest request)
    {
        var url = CreateUrlWithParams($"v1/Wallets/walletBalance", request, true);
        var response = await GetAsync(url);
        var walletbalanceList = await response.Content.ReadFromJsonAsync<WalletBalanceResponse>();
        if (!CanSeeSensitiveData())
        {
            walletbalanceList.WalletBalances.Items.ForEach(s =>
            {
                s.Firstname = SensitiveDataHelper.MaskSensitiveData("Name", s.Firstname);
                s.Lastname = SensitiveDataHelper.MaskSensitiveData("Name", s.Lastname);
            });
        }
        return walletbalanceList ?? throw new InvalidOperationException();
    }

    public async Task<WalletBalanceDailyResponse> GetWalletBalanceDailyAsync(GetWalletBalancesDailyRequest request)
    {
        var url = CreateUrlWithParams($"v1/Wallets/wallet-balances-daily", request, true);
        var response = await GetAsync(url);
        var walletBalanceDailyList = await response.Content.ReadFromJsonAsync<WalletBalanceDailyResponse>();
        return walletBalanceDailyList ?? throw new InvalidOperationException();
    }
}
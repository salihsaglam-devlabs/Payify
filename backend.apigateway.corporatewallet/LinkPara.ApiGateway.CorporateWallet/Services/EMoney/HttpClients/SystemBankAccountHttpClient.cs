using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Responses;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.HttpClients;

public class SystemBankAccountHttpClient : HttpClientBase, ISystemBankAccountHttpClient
{
    public SystemBankAccountHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task<List<SystemBankAccountDto>> GetSystemBankAccountsAsync()
    {
        var response = await GetAsync($"v1/SystemBankAccounts");
        var accounts = await response.Content.ReadFromJsonAsync<List<SystemBankAccountDto>>();
        return accounts ?? throw new InvalidOperationException();
    }
}
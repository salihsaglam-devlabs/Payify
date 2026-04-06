using LinkPara.HttpProviders.Emoney.Models;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;

namespace LinkPara.HttpProviders.Emoney;
public class WalletService : HttpClientBase, IWalletService
{
    public WalletService(HttpClient client, IHttpContextAccessor httpContextAccessor) 
        : base(client, httpContextAccessor)
    {
    }

    public async Task<UpdateBalanceResponse> UpdateBalanceAsync(UpdateBalanceRequest request)
    {
        var response = await PutAsJsonAsync($"v1/Wallets/update-balance", request);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }

        var updateBalanceResponse = await response.Content.ReadFromJsonAsync<UpdateBalanceResponse>();

        return updateBalanceResponse ?? throw new InvalidOperationException();
    }

    public async Task<ValidateWalletServiceResponse> ValidateWalletAsync(ValidateWalletRequest request)
    {
        var response = await PostAsJsonAsync("v1/Wallets/validate-wallet", request);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }

        var validateWalletResponse = await response.Content.ReadFromJsonAsync<ValidateWalletServiceResponse>();

        return validateWalletResponse ?? throw new InvalidOperationException();
    }
}

using LinkPara.HttpProviders.Emoney.Models;
using LinkPara.HttpProviders.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Identity.Client;
using System.Net.Http.Json;
using System.Security.Policy;

namespace LinkPara.HttpProviders.Emoney;

public class AccountService : HttpClientBase, IAccountService
{
    public AccountService(HttpClient client, IHttpContextAccessor httpContextAccessor) :
        base(client, httpContextAccessor)
    {
    }

    public async Task<AccountResponse> GetAccountDetailAsync(GetAccountDetailRequest request)
    {
        var url = GetQueryString.CreateUrlWithParams($"v1/Accounts/detail", request);
        var response = await GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }

        var accountResponse = await response.Content.ReadFromJsonAsync<AccountResponse>();

        return accountResponse ?? throw new InvalidOperationException();
    }

    public async Task<List<AccountUserResponse>> GetAccountUserListAsync(Guid accountId)
    {
        var response = await GetAsync($"v1/Accounts/{accountId}/users/");

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }
        var accountUserResponse = await response.Content.ReadFromJsonAsync<List<AccountUserResponse>>();

        return accountUserResponse ?? throw new InvalidOperationException();
    }

    public async Task PatchAccountAsync(Guid accountId, JsonPatchDocument<PatchAccountDto> patchAccountDto)
    {
        var response = await PatchAsync($"v1/Accounts/{accountId}", patchAccountDto);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }
    }

    public async Task PatchAccountUserAsync(Guid accountId, Guid accountUserId, JsonPatchDocument<PatchAccountUserDto> patchAccountUserDto)
    {
        var response = await PatchAsync($"v1/Accounts/{accountId}/users/{accountUserId}", patchAccountUserDto);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }
    }

    public async Task<ParentAccountResponse> GetParentAccountByUserIdAsync(Guid userId)
    {
        var response = await GetAsync($"v1/Accounts/{userId}/getParentAccount");
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }

        var custodyAccountsResponse = await response.Content.ReadFromJsonAsync<ParentAccountResponse>();

        return custodyAccountsResponse ?? throw new InvalidOperationException();
    }
}

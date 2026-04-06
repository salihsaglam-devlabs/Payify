using LinkPara.ApiGateway.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Services.Emoney.Models.Responses;
using Microsoft.AspNetCore.JsonPatch;
using System.Text.Json;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using System.Collections.Generic;
using Elastic.Apm.Api;

namespace LinkPara.ApiGateway.Services.Emoney.HttpClients;

public class EmoneyAccountHttpClient : HttpClientBase, IEmoneyAccountHttpClient
{
    public EmoneyAccountHttpClient(HttpClient client,
        IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
    {

    }

    public async Task CreateAccountAsync(CreateEmoneyAccountRequest request)
    {
        await PostAsJsonAsync($"v1/Accounts", request);
    }

    public async Task CreateAccountUserAsync(CreateEmoneyAccountUserRequest request)
    {
        await PostAsJsonAsync($"v1/Accounts/{request.AccountId}/Users", request);
    }

    public async Task<AccountDto> GetAccountByUserIdAsync(Guid userId)
    {
        var response = await GetAsync($"v1/Accounts/detail?userid={userId}");
        var responseString = await response.Content.ReadAsStringAsync();
        var account = JsonSerializer.Deserialize<AccountDto>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return account ?? throw new InvalidOperationException();
    }

    public async Task<AccountDto> GetAccountByWalletNumberAsync(string walletNumber)
    {
        var response = await GetAsync($"v1/Accounts/detail?userid={Guid.Empty}&walletNumber={walletNumber}");
        var responseString = await response.Content.ReadAsStringAsync();
        var account = JsonSerializer.Deserialize<AccountDto>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return account ?? throw new InvalidOperationException();
    }

    public async Task PatchAccountAsync(Guid accountId,
        JsonPatchDocument<UpdateAccountRequest> request)
    {
        await PatchAsync($"v1/Accounts/{accountId}", request);
    }

    public async Task ValidateAccountUserIdentityAsync(ValidateIdentityRequest request, string userId)
    {
        if (!request.UserId.ToString().Equals(userId))
        {
            throw new ForbiddenAccessException();
        }

        await PostAsJsonAsync($"v1/Accounts/validate-identity", request);
    }

    public async Task<PaginatedList<CustodyAccountResponse>> GetCustodyAccountListAsync(GetCustodyAccountListRequest request)
    {
        var url = CreateUrlWithParams($"v1/Accounts/getCustodyAccountList", request, true);
        var response = await GetAsync(url);
        var accounts = await response.Content.ReadFromJsonAsync<PaginatedList<CustodyAccountResponse>>();
        return accounts ?? throw new InvalidOperationException();
    }
}

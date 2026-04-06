using LinkPara.ApiGateway.Boa.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Boa.Services.Emoney.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.JsonPatch;
using System.Text.Json;

namespace LinkPara.ApiGateway.Boa.Services.Emoney.HttpClients;

public class EmoneyAccountHttpClient : HttpClientBase, IEmoneyAccountHttpClient
{
    public EmoneyAccountHttpClient(HttpClient client,
        IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
    {

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

    public async Task PatchAccountAsync(Guid accountId, JsonPatchDocument<UpdateAccountRequest> request)
    {
        await PatchAsync($"v1/Accounts/{accountId}", request);
    }

    public async Task CreateAccountAsync(CreateEmoneyAccountRequest request)
    {
        await PostAsJsonAsync($"v1/Accounts", request);
    }
}

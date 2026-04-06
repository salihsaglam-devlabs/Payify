using LinkPara.ApiGateway.BackOffice.Commons.Extensions;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;
using LinkPara.ApiGateway.BackOffice.Services.Identity.HttpClients;
using LinkPara.SharedModels.Pagination;
using System.Text.Json;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;

public class CorporateWalletHttpClient : HttpClientBase, ICorporateWalletHttpClient
{
    private readonly IUserHttpClient _userHttpClient;
    public CorporateWalletHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor, IUserHttpClient userHttpClient)
        : base(client, httpContextAccessor)
    {
        _userHttpClient = userHttpClient;
    }

    public async Task ActivateCorporateWalletAccountAsync(Guid id)
    {
        await PutAsJsonAsync($"v1/CorporateWallets/accounts/activate/{id}", new object());
    }

    public async Task ActivateCorporateWalletUserAsync(ActivateCorporateWalletUserRequest request)
    {
        await PutAsJsonAsync("v1/CorporateWallets/users/activate", request);
    }

    public async Task AddCorporateWalletUserAsync(AddCorporateWalletUserRequest request)
    {
        await PostAsJsonAsync("v1/CorporateWallets/users", request);
    }

    public async Task DeactivateCorporateWalletAccountAsync(Guid id)
    {
        await PutAsJsonAsync($"v1/CorporateWallets/accounts/deactivate/{id}", new object());
    }

    public async Task DeactivateCorporateWalletUserAsync(DeactivateCorporateWalletUserRequest request)
    {
        await PutAsJsonAsync("v1/CorporateWallets/users/deactivate", request);
    }

    public async Task<CorporateAccountDto> GetCorporateWalletAccountAsync(Guid id)
    {
        var response = await GetAsync($"v1/CorporateWallets/accounts/{id}");
        var corporateWalletAccount = await response.Content.ReadFromJsonAsync<CorporateAccountDto>();
        return corporateWalletAccount ?? throw new InvalidOperationException();
    }

    public async Task<PaginatedList<CorporateAccountDto>> GetCorporateWalletAccountsAsync(GetCorporateAccountsRequest request)
    {
        var queryString = request.GetQueryString();
        var response = await GetAsync($"v1/CorporateWallets/accounts{queryString}");
        var responseString = await response.Content.ReadAsStringAsync();
        var corporateWalletAccounts = JsonSerializer.Deserialize<PaginatedList<CorporateAccountDto>>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return corporateWalletAccounts ?? throw new InvalidOperationException();
    }

    public async Task<CorporateWalletUserDto> GetCorporateWalletUserAsync(Guid id)
    {
        var response = await GetAsync($"v1/CorporateWallets/users/{id}");
        var corporateWalletUser = await response.Content.ReadFromJsonAsync<CorporateWalletUserDto>();

        var user = await _userHttpClient.GetUserByIdAsync(corporateWalletUser.UserId);

        corporateWalletUser.LockOutEnd = user.LockoutEnd;

        return corporateWalletUser ?? throw new InvalidOperationException();
    }

    public async Task<PaginatedList<CorporateWalletUserDto>> GetCorporateWalletUsersAsync(GetCorporateWalletUsersRequest request)
    {
        var queryString = request.GetQueryString();
        var response = await GetAsync($"v1/CorporateWallets/users{queryString}");
        var responseString = await response.Content.ReadAsStringAsync();
        var corporateWalletUsers = JsonSerializer.Deserialize<PaginatedList<CorporateWalletUserDto>>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return corporateWalletUsers ?? throw new InvalidOperationException();
    }

    public async Task UpdateCorporateWalletAccountAsync(UpdateCorporateAccountRequest request)
    {
        await PutAsJsonAsync("v1/CorporateWallets/accounts", request);
    }

    public async Task UpdateCorporateWalletUserAsync(UpdateCorporateWalletUserRequest request)
    {
        await PutAsJsonAsync("v1/CorporateWallets/users", request);
    }
}

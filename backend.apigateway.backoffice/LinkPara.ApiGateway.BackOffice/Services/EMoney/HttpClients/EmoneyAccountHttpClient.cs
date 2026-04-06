using LinkPara.ApiGateway.BackOffice.Commons.Helpers;
using LinkPara.ApiGateway.BackOffice.Commons.Models.AuthorizationModels;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;

public class EmoneyAccountHttpClient : HttpClientBase, IEmoneyAccountHttpClient
{
    private readonly IConfiguration _configuration;

    public EmoneyAccountHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        : base(client, httpContextAccessor)
    {
        _configuration = configuration;
    }

    public async Task<AccountDto> GetAccountByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/Accounts/{id}");
        var result = await response.Content.ReadFromJsonAsync<AccountDto>();

        var accountPhoneNumber = string.Concat(result.PhoneCode, result.PhoneNumber);

        var defaultUsers = GetDefaultUsers();

        if (defaultUsers != null && defaultUsers.Any(s => s.PhoneNumber.Equals(accountPhoneNumber)))
        {
            throw new NotFoundException(nameof(AccountDto), id);
        }

        if (!CanSeeSensitiveData())
        {
            result.IdentityNumber = SensitiveDataHelper.MaskSensitiveData("IdentityNumber", result.IdentityNumber);
        }

        return result;
    }

    public async Task<PaginatedList<AccountDto>> GetAccountListAsync(GetAccountListRequest request)
    {
        var url = CreateUrlWithParams($"v1/Accounts", request, true);
        var response = await GetAsync(url);
        var results = await response.Content.ReadFromJsonAsync<PaginatedList<AccountDto>>();

        var defaultUsers = GetDefaultUsers();

        if (defaultUsers != null && defaultUsers.Any())
        {
            var filteredAccounts = results.Items
                .Where(s =>
                    !defaultUsers.Any(d =>
                        string.Concat(s.PhoneNumber, s.PhoneCode) == d.PhoneNumber))
                .ToList();

            results.Items = filteredAccounts;
        }

        if (!CanSeeSensitiveData())
        {
            results.Items.ForEach(s =>
            {
                s.IdentityNumber = SensitiveDataHelper.MaskSensitiveData("IdentityNumber", s.IdentityNumber);
            });
        }

        return results;
    }

    public async Task<List<AccountUserDto>> GetAccountUserListAsync(Guid accountId)
    {
        var response = await GetAsync($"v1/Accounts/{accountId}/Users");
        return await response.Content.ReadFromJsonAsync<List<AccountUserDto>>();
    }

    public async Task<List<WalletDto>> GetAccountWalletListAsync(Guid accountId)
    {
        var response = await GetAsync($"v1/Accounts/{accountId}/Wallets");
        return await response.Content.ReadFromJsonAsync<List<WalletDto>>();
    }

    public async Task<AccountDto> PatchAsync(Guid accountId, JsonPatchDocument<UpdateAccountRequest> request)
    {
        var response = await PatchAsync($"v1/Accounts/{accountId}", request);
        return await response.Content.ReadFromJsonAsync<AccountDto>();
    }

    public async Task<PaginatedList<AccountUserDto>> GetAllAccountUserAsync(GetAllAccountUserRequest request)
    {
        var url = CreateUrlWithParams($"v1/Accounts/accountUsers", request, true);
        var response = await GetAsync(url);
        var accountUsers = await response.Content.ReadFromJsonAsync<PaginatedList<AccountUserDto>>();
        return accountUsers ?? throw new InvalidOperationException();
    }

    private List<DefaultUser> GetDefaultUsers()
    {
        var defaultUsers = _configuration.GetSection("DefaultUsers").Get<List<DefaultUser>>();

        return defaultUsers;
    }

    public async Task<List<AccountKycChangeDto>> GetAccountKycChangesByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/Accounts/{id}/kyc-changes");
        return await response.Content.ReadFromJsonAsync<List<AccountKycChangeDto>>();
    }

    public async Task<PaginatedList<CustodyAccountResponse>> GetCustodyAccountListAsync(GetCustodyAccountListRequest request)
    {
        var url = CreateUrlWithParams($"v1/Accounts/getCustodyAccountList", request, true);
        var response = await GetAsync(url);
        var accounts = await response.Content.ReadFromJsonAsync<PaginatedList<CustodyAccountResponse>>();
        return accounts ?? throw new InvalidOperationException();
    }

    public async Task DeactivateAccountAsync(DeactivateAccountRequest request)
    {
        await PostAsJsonAsync($"v1/Accounts/deactivate-account", request);
    }
}

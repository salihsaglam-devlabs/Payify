using LinkPara.ApiGateway.CorporateWallet.Commons.Extensions;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Responses;
using LinkPara.SharedModels.Pagination;
using System.Security.Claims;
using System.Text.Json;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.HttpClients;

public class CorporateWalletHttpClient : HttpClientBase, ICorporateWalletHttpClient
{
    private readonly Guid UserId;

    private readonly IEmoneyAccountHttpClient _emoneyAccountHttpClient;
    public CorporateWalletHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor, IEmoneyAccountHttpClient emoneyAccountHttpClient)
        : base(client, httpContextAccessor)
    {

        _emoneyAccountHttpClient = emoneyAccountHttpClient;

        var loggedUserId = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        if (Guid.TryParse(loggedUserId, out var userId))
        {
            UserId = userId;
        }
    }

    public async Task ActivateCorporateWalletUserAsync(ActivateCorporateWalletUserRequest request)
    {
        var account = await _emoneyAccountHttpClient.GetAccountByUserIdAsync(UserId);

        var serviceRequest = new ActivateCorporateWalletUserServiceRequest
        {
            AccountId = account.Id,
            AccountUserId = request.AccountUserId
        };

        await PutAsJsonAsync("v1/CorporateWallets/users/activate", serviceRequest);
    }

    public async Task AddCorporateWalletUserAsync(AddCorporateWalletUserRequest request)
    {
        var account = await _emoneyAccountHttpClient.GetAccountByUserIdAsync(UserId);

        var serviceRequest = new AddCorporateWalletUserServiceRequest
        {
            AccountId = account.Id,
            BirthDate = request.BirthDate,
            Email = request.Email,
            Firstname = request.Firstname,
            Lastname = request.Lastname,
            PhoneCode = request.PhoneCode,
            PhoneNumber = request.PhoneNumber,
            Roles = request.Roles
        };

        await PostAsJsonAsync("v1/CorporateWallets/users", serviceRequest);
    }

    public async Task DeactivateCorporateWalletUserAsync(DeactivateCorporateWalletUserRequest request)
    {
        var account = await _emoneyAccountHttpClient.GetAccountByUserIdAsync(UserId);

        var serviceRequest = new DeactivateCorporateWalletUserServiceRequest
        {
            AccountId = account.Id,
            AccountUserId = request.AccountUserId
        };

        await PutAsJsonAsync("v1/CorporateWallets/users/deactivate", serviceRequest);
    }

    public async Task<CorporateAccountDto> GetCorporateWalletAccountAsync()
    {
        var account = await _emoneyAccountHttpClient.GetAccountByUserIdAsync(UserId);

        var response = await GetAsync($"v1/CorporateWallets/accounts/{account.Id}");
        var corporateWalletAccount = await response.Content.ReadFromJsonAsync<CorporateAccountDto>();
        return corporateWalletAccount ?? throw new InvalidOperationException();
    }

    public async Task<CorporateWalletUserDto> GetCorporateWalletUserAsync(Guid id)
    {
        var response = await GetAsync($"v1/CorporateWallets/users/{id}");
        var corporateWalletUser = await response.Content.ReadFromJsonAsync<CorporateWalletUserDto>();
        return corporateWalletUser ?? throw new InvalidOperationException();
    }

    public async Task<PaginatedList<CorporateWalletUserDto>> GetCorporateWalletUsersAsync(GetCorporateWalletUsersRequest request)
    {
        var account = await _emoneyAccountHttpClient.GetAccountByUserIdAsync(UserId);

        var serviceRequest = new GetCorporateWalletUsersServiceRequest
        {
            AccountId = account.Id,
            Email = request.Email,
            FullName = request.FullName,
            OrderBy = request.OrderBy,
            Page = request.Page,
            PhoneCode = request.PhoneCode,
            PhoneNumber = request.PhoneNumber,
            Q = request.Q,
            RecordStatus = request.RecordStatus,
            Size = request.Size,
            SortBy = request.SortBy
        };

        var queryString = serviceRequest.GetQueryString();
        var response = await GetAsync($"v1/CorporateWallets/users{queryString}");
        var responseString = await response.Content.ReadAsStringAsync();
        var corporateWalletUsers = JsonSerializer.Deserialize<PaginatedList<CorporateWalletUserDto>>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return corporateWalletUsers ?? throw new InvalidOperationException();
    }

    public async Task UpdateCorporateWalletUserAsync(UpdateCorporateWalletUserRequest request)
    {
        var account = await _emoneyAccountHttpClient.GetAccountByUserIdAsync(UserId);

        var serviceRequest = new UpdateCorporateWalletUserServiceRequest
        {
            AccountUserId = request.AccountUserId,
            AccountId = account.Id,
            Email = request.Email,
            PhoneCode = request.PhoneCode,
            PhoneNumber = request.PhoneNumber,
            Roles = request.Roles
        };

        await PutAsJsonAsync("v1/CorporateWallets/users", serviceRequest);
    }
}

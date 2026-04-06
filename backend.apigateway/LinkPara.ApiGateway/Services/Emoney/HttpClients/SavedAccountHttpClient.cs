using System.Security.Claims;
using Elastic.Apm.Api;
using LinkPara.ApiGateway.Commons.Helpers;
using LinkPara.ApiGateway.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Services.Emoney.Models.Responses;
using LinkPara.HttpProviders.Vault;

namespace LinkPara.ApiGateway.Services.Emoney.HttpClients;

public class SavedAccountHttpClient : HttpClientBase, ISavedAccountHttpClient
{
    private readonly IServiceRequestConverter _serviceRequestConverter;
    private readonly IConfiguration _configuration;
    private readonly IVaultClient _vaultClient;
    private readonly IStringMasking _stringMasking;
    private Guid UserId;

    public SavedAccountHttpClient(HttpClient client
                                , IHttpContextAccessor httpContextAccessor
                                , IServiceRequestConverter serviceRequestConverter
                                , IConfiguration configuration
                                , IVaultClient vaultClient
                                , IStringMasking stringMasking)
       : base(client, httpContextAccessor)
    {
        _serviceRequestConverter = serviceRequestConverter;
        _configuration = configuration;
        _vaultClient = vaultClient;
        _stringMasking = stringMasking;
        UserId = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) is not null
      ? Guid.TryParse(httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : Guid.Empty
      : Guid.Empty;
    }

    public async Task<List<SavedBankAccountDto>> GetBankAccountsAsync(string userId)
    {
        var response = await GetAsync($"v1/SavedAccounts/bank-accounts?UserId={userId}");
        var banks = await response.Content.ReadFromJsonAsync<List<SavedBankAccountDto>>();

        var baseGatewayUrl = _vaultClient.GetSecretValue<string>("SharedSecrets", "ServiceUrls", "ApiGateway");
        if (baseGatewayUrl.EndsWith("/"))
        {
            baseGatewayUrl = baseGatewayUrl.Remove(baseGatewayUrl.Length - 1);
        }

        if (banks != null)
        {
            foreach (var bank in banks)
            {
                if (bank.Bank is not null)
                {
                    bank.Bank.LogoUrl = string.IsNullOrEmpty(bank.Bank.LogoUrl) ? null : $"{baseGatewayUrl}{bank.Bank.LogoUrl}";
                }
            }
        }

        return banks ?? throw new InvalidOperationException();
    }

    public async Task SaveBankAccountAsync(SaveBankAccountRequest request)
    {
        var clientRequest = _serviceRequestConverter.Convert<SaveBankAccountRequest, SaveBankAccountServiceRequest>(request);
        var response = await PostAsJsonAsync($"v1/SavedAccounts/bank-accounts", clientRequest);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task UpdateBankAccountAsync(Guid id, UpdateBankAccountRequest request)
    {
        var clientRequest = _serviceRequestConverter.Convert<UpdateBankAccountRequest, UpdateBankAccountServiceRequest>(request);
        var response = await PutAsJsonAsync($"v1/SavedAccounts/bank-accounts/{id}", clientRequest);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task UpdateWalletAccountAsync(Guid id, UpdateWalletAccountRequest request)
    {
        var clientRequest = _serviceRequestConverter.Convert<UpdateWalletAccountRequest, UpdateWalletAccountServiceRequest>(request);
        var response = await PutAsJsonAsync($"v1/SavedAccounts/wallets/{id}", clientRequest);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task DeleteSavedAccountAsync(Guid id, string userId)
    {
        var response = await DeleteAsync($"v1/SavedAccounts/{id}?UserId={userId}");
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task SaveWalletAccountAsync(SaveWalletAccountRequest request)
    {
        var clientRequest = _serviceRequestConverter.Convert<SaveWalletAccountRequest, SaveWalletAccountServiceRequest>(request);
        var response = await PostAsJsonAsync($"v1/SavedAccounts/wallets", clientRequest);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task<List<SavedWalletAccountDto>> GetWalletAccountsAsync(string userId)
    {
        var response = await GetAsync($"v1/SavedAccounts/wallets?UserId={userId}");
        var wallets = await response.Content.ReadFromJsonAsync<List<SavedWalletAccountDto>>();
        return wallets ?? throw new InvalidOperationException();
    }

    public async Task<SavedBankAccountDto> GetBankAccountByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/SavedAccounts/bank-accounts/{id}");
        var bank = await response.Content.ReadFromJsonAsync<SavedBankAccountDto>();

        var baseGatewayUrl = _vaultClient.GetSecretValue<string>("SharedSecrets", "ServiceUrls", "ApiGateway");
        if (baseGatewayUrl.EndsWith("/"))
        {
            baseGatewayUrl = baseGatewayUrl.Remove(baseGatewayUrl.Length - 1);
        }

        if (bank != null && bank.Bank is not null)
        {
            bank.Bank.LogoUrl = string.IsNullOrEmpty(bank.Bank.LogoUrl) ? null : $"{baseGatewayUrl}{bank.Bank.LogoUrl}";
        }

        return bank ?? throw new InvalidOperationException();
    }

    public async Task<SavedWalletAccountDetailDto> GetWalletAccountByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/SavedAccounts/wallets/{id}");
        var wallet = await response.Content.ReadFromJsonAsync<SavedWalletAccountDetailDto>();
        wallet.WalletOwnerName = await MaskNameIfNeededAsync(wallet?.WalletNumber, wallet?.WalletOwnerName, UserId);
        return wallet ?? throw new InvalidOperationException();
    }

    private async Task<string> MaskNameIfNeededAsync(string walletNumber, string name, Guid loggedUserId)
    {
        if (string.IsNullOrEmpty(walletNumber) || string.IsNullOrEmpty(name))
        {
            return name;
        }

        var accountResponse = await GetAsync($"v1/accounts/detail?UserId={Guid.Empty}&WalletNumber={walletNumber}");
        var account = await accountResponse.Content.ReadFromJsonAsync<AccountDto>();
        var accountUsersResponse = await GetAsync($"v1/accounts/{account.Id}/users/");
        var accountUsers = await accountUsersResponse.Content.ReadFromJsonAsync<List<AccountUserDto>>();

        if (account?.IsNameMaskingEnabled == true && !accountUsers.Any(x => x.UserId == loggedUserId))
        {
            return await _stringMasking.MaskStringAsync(name);
        }

        return name;
    }
}

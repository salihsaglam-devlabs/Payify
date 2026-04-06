using LinkPara.ApiGateway.CorporateWallet.Commons.Helpers;
using LinkPara.ApiGateway.CorporateWallet.Services;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.HttpClients;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Responses;
using LinkPara.HttpProviders.Vault;

namespace LinkPara.ApiGateway.Services.Emoney.HttpClients;

public class SavedAccountHttpClient : HttpClientBase, ISavedAccountHttpClient
{
    private readonly IServiceRequestConverter _serviceRequestConverter;
    private readonly IVaultClient _vaultClient;

    public SavedAccountHttpClient(HttpClient client
                                , IHttpContextAccessor httpContextAccessor
                                , IServiceRequestConverter serviceRequestConverter
                                , IVaultClient vaultClient)
       : base(client, httpContextAccessor)
    {
        _serviceRequestConverter = serviceRequestConverter;
        _vaultClient = vaultClient;
    }

    public async Task<List<SavedBankAccountDto>> GetBankAccountsAsync(string userId)
    {
        var response = await GetAsync($"v1/SavedAccounts/bank-accounts?UserId={userId}");
        var banks = await response.Content.ReadFromJsonAsync<List<SavedBankAccountDto>>();

        var baseGatewayUrl = _vaultClient.GetSecretValue<string>("SharedSecrets", "ServiceUrls", "ApiGateway");

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
        return wallet ?? throw new InvalidOperationException();
    }
}

using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Responses;
using LinkPara.HttpProviders.Vault;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.HttpClients
{
    public class BankHttpClient : HttpClientBase, IBankHttpClient
    {
        private readonly IVaultClient _vaultClient;

        public BankHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor,
            IVaultClient vaultClient)
            : base(client, httpContextAccessor)
        {
            _vaultClient = vaultClient;
        }

        public async Task<List<BankDto>> GetBanksAsync(string iban = null)
        {
            var response = await GetAsync($"v1/banks?iban={iban}");
            var banks = await response.Content.ReadFromJsonAsync<List<BankDto>>();

            var baseGatewayUrl = _vaultClient.GetSecretValue<string>("SharedSecrets", "ServiceUrls", "CorporateWalletGateway");

            if (banks != null)
            {
                foreach (var bank in banks)
                {
                    bank.LogoUrl = string.IsNullOrEmpty(bank.LogoUrl) ? null : $"{baseGatewayUrl}{bank.LogoUrl}";
                }
            }

            return banks ?? throw new InvalidOperationException();
        }
    }
}



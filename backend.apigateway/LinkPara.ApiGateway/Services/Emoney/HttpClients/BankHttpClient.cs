using LinkPara.ApiGateway.Services.Emoney.Models.Responses;
using LinkPara.HttpProviders.Vault;

namespace LinkPara.ApiGateway.Services.Emoney.HttpClients
{
    public class BankHttpClient : HttpClientBase, IBankHttpClient
    {
        private readonly IConfiguration _configuration;
        private readonly IVaultClient _vaultClient;

        public BankHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            IVaultClient vaultClient)
            : base(client, httpContextAccessor)
        {
            _configuration = configuration;
            _vaultClient = vaultClient;
        }

        public async Task<List<BankDto>> GetBanksAsync(string iban = null)
        {
            var response = await GetAsync($"v1/banks?iban={iban}");
            var banks = await response.Content.ReadFromJsonAsync<List<BankDto>>();

            var baseGatewayUrl = _vaultClient.GetSecretValue<string>("SharedSecrets", "ServiceUrls", "ApiGateway");
            if (baseGatewayUrl.EndsWith("/"))
            {
                baseGatewayUrl = baseGatewayUrl.Remove(baseGatewayUrl.Length - 1);
            }

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



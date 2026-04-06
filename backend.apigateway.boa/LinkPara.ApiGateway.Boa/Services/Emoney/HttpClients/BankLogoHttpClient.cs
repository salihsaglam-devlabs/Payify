using LinkPara.ApiGateway.Boa.Services.Emoney.Models.Responses;

namespace LinkPara.ApiGateway.Boa.Services.Emoney.HttpClients;

public class BankLogoHttpClient : HttpClientBase, IBankLogoHttpClient
{
    public BankLogoHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
    : base(client, httpContextAccessor)
    {
    }

    public async Task<BankLogoDto> GetBankLogoAsync(Guid bankId)
    {
        var response = await GetAsync($"v1/BankLogos?BankId={bankId}");
        var bankLogo = await response.Content.ReadFromJsonAsync<BankLogoDto>();
        return bankLogo ?? throw new InvalidOperationException();
    }
}
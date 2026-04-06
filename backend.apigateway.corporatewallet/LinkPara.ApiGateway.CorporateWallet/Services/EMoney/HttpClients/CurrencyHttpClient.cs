using LinkPara.ApiGateway.CorporateWallet.Commons.Extensions;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Responses;
using System.Text.Json;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.HttpClients;

public class CurrencyHttpClient : HttpClientBase, ICurrencyHttpClient
{
    public CurrencyHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
    : base(client, httpContextAccessor)
    {
    }

    public async Task<List<CurrencyDto>> GetAllAsync(CurrenciesFilterRequest request)
    {
        var queryString = request.GetQueryString();
        var response = await GetAsync($"v1/Currencies" + queryString);
        var responseString = await response.Content.ReadAsStringAsync();
        var currencyList = JsonSerializer.Deserialize<List<CurrencyDto>>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return currencyList ?? throw new InvalidOperationException();
    }
}

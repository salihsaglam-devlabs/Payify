using LinkPara.HttpProviders.MoneyTransfer.Models;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace LinkPara.HttpProviders.MoneyTransfer;

public class BankApiService : HttpClientBase, IBankApiService
{
    public BankApiService(HttpClient client, IHttpContextAccessor httpContextAccessor) 
        : base(client, httpContextAccessor)
    {
    }

    public async Task<BankApiDto> GetByBankCodeAsync(int code)
    {
        var response = await GetAsync($"v1/BankApis/{code}");

        var responseString = await response.Content.ReadAsStringAsync();

        var bankApiResponse = JsonSerializer.Deserialize<BankApiDto>(responseString,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        return bankApiResponse;
    }
}

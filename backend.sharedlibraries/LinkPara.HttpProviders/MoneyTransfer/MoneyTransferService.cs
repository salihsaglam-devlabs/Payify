using LinkPara.HttpProviders.MoneyTransfer.Models;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using System.Globalization;

namespace LinkPara.HttpProviders.MoneyTransfer;

public class MoneyTransferService : HttpClientBase, IMoneyTransferService
{
    public MoneyTransferService(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task<CheckIbanResponse> CheckIbanAsync(string iban)
    {
        var response = await GetAsync($"v1/Transfers/{iban}");

        var responseString = await response.Content.ReadAsStringAsync();

        var checkIbanResponse = JsonSerializer.Deserialize<CheckIbanResponse>(responseString,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        return checkIbanResponse;
    }

    public async Task<GetTransferBankResponse> GetTransferBankAsync(GetTransferBankRequest request)
    {
        var queryString =
            $"Amount={request.Amount.ToString(CultureInfo.InvariantCulture)}" +
            $"&ReceiverIBAN={request.ReceiverIBAN}" +
            $"&CurrencyCode={request.CurrencyCode}";

        var response = await GetAsync($"v1/Transfers?{queryString}");

        var responseString = await response.Content.ReadAsStringAsync();

        var previewResponse = JsonSerializer.Deserialize<GetTransferBankResponse>(responseString,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        return previewResponse;
    }

    public async Task<SaveTransferResponse> SaveTransferAsync(SaveTransferRequest request)
    {
        var response = await PostAsJsonAsync("v1/Transfers", request);

        var transferResponse = await response.Content.ReadFromJsonAsync<SaveTransferResponse>();

        return transferResponse;
    }

    public async Task UpdateTransferBankAsync(UpdateTransferBankRequest request)
    {
        await PutAsJsonAsync("v1/Transfers", request);
    }
}
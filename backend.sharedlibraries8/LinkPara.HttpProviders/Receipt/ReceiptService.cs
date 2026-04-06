
using LinkPara.HttpProviders.Receipt.Models;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace LinkPara.HttpProviders.Receipt;

public class ReceiptService : HttpClientBase, IReceiptService
{
    public ReceiptService(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {

    }

    public async Task<ReceiptDto> GetReceiptByIdAsync(GetReceiptByIdRequest request)
    {
        var response = await GetAsync($"v1/ReceiptDetail/{request.TransactionId}");
        var responseString = await response.Content.ReadAsStringAsync();

        var receiptInfoResponse = JsonSerializer.Deserialize<ReceiptDto>(responseString,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        return receiptInfoResponse;
    }

    public async Task<CreateReceiptResponse> CreateReceiptAsync(CreateReceiptRequest request)
    {
        var response = await PostAsJsonAsync($"v1/ReceiptDetail", request);

        if (response.IsSuccessStatusCode)
        {
            var responseString = await response.Content.ReadAsStringAsync();

            var mappedResponse = JsonSerializer.Deserialize<CreateReceiptResponse>(responseString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return mappedResponse;
        }
        throw new InvalidOperationException();
    }
}
using LinkPara.HttpProviders.Fraud.Models;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;

namespace LinkPara.HttpProviders.Fraud;

public class FraudTransactionService : HttpClientBase, IFraudTransactionService
{

    public FraudTransactionService(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {

    }

    public async Task<TransactionResponse> ExecuteTransaction(FraudCheckRequest request)
    {
        var response = await PostAsJsonAsync("v1/TransactionMonitorings", request);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }

        var transactionResponse = await response.Content.ReadFromJsonAsync<TransactionResponse>();

        return transactionResponse ?? throw new InvalidOperationException();
    }
}

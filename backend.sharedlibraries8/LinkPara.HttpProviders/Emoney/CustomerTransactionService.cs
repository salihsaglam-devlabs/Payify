using LinkPara.HttpProviders.Emoney.Models;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;

namespace LinkPara.HttpProviders.Emoney;

public class CustomerTransactionService : HttpClientBase, ICustomerTransactionService
{
    public CustomerTransactionService(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task<CustomerTransactionResponse> GetTransactionsByCustomerTransactionIdAsync(CustomerTransactionRequest request)
    {
        var response = await GetAsync($"v1/Transactions/getByCustomerTransactionId/{request.CustomerTransactionId}");

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }

        var customerTransactions = await response.Content.ReadFromJsonAsync<List<CustomerTransactionModel>>();

        return new CustomerTransactionResponse
        {
            CustomerTransactions = customerTransactions ?? throw new InvalidOperationException()
        };
    }
}

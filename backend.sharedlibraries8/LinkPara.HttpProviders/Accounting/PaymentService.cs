using LinkPara.HttpProviders.BusinessParameter.Models;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace LinkPara.HttpProviders.Accounting;

public class PaymentService : HttpClientBase, IPaymentService
{
    
    public PaymentService(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {

    }
    public async Task CancelPaymentAsync(Guid clientReferenceId)
    {
        var response = await DeleteAsync($"v1/Payments/{clientReferenceId}");

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }
    }
}

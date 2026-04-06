using LinkPara.HttpProviders.Emoney.Models;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;

namespace LinkPara.HttpProviders.Emoney;

public class OnUsPaymentService : HttpClientBase, IOnUsPaymentService
{

    public OnUsPaymentService(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {

    }

    public async Task<OnUsPaymentResponse> InitOnUsPaymentAsync(InitOnUsPaymentRequest request)
    {
        var response = await PostAsJsonAsync("v1/OnUsPayment/init", request);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }

        var initOnUsResponse = await response.Content.ReadFromJsonAsync<OnUsPaymentResponse>();

        return initOnUsResponse; 
    }

    public async Task OnUsPaymentUpdateStatusAsync(OnUsPaymentUpdateStatusRequest request)
    {
        var response = await PostAsJsonAsync("v1/OnUsPayment/updatestatus", request);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }       
    }
}

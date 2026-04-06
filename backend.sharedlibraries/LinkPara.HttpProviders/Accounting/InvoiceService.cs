using LinkPara.HttpProviders.Accounting.Models;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;

namespace LinkPara.HttpProviders.Accounting;

public class InvoiceService : HttpClientBase, IInvoiceService
{
    public InvoiceService(HttpClient client, IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor) { }

    public async Task<InvoiceDto> GetInvoiceAsync(Guid clientReferenceId)
    {
        var response = await GetAsync($"v1/Invoice/{clientReferenceId}");

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<InvoiceDto>();
        }
        else
        {
            throw new InvalidOperationException();
        }
    }
}
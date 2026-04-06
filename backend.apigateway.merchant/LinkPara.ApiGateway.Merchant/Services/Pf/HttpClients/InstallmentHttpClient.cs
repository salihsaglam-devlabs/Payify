using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using System.Text.Json;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;

public class InstallmentHttpClient : HttpClientBase, IInstallmentHttpClient
{
    public InstallmentHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) 
        : base(client, httpContextAccessor)
    {
    }
    public async Task<InstallmentPricingResponse> CalcuateInstallmentPricings(InstallmentPricingRequest query)
    {
        var url = CreateUrlWithProperties("v1/Installments", query);
        var response = await GetAsync(url);
        var acquireBanks = await response.Content.ReadFromJsonAsync<InstallmentPricingResponse>();
        return acquireBanks ?? throw new InvalidOperationException();
    }

    public async Task<InstallmentsManualPaymentPageResponse> GetManualPaymentPageInstallments(Guid merchantId)
    {
        var response = await GetAsync($"v1/Installments/ManualPaymentPage/{merchantId}");

        var responseString = await response.Content.ReadAsStringAsync();

        var user = JsonSerializer.Deserialize<InstallmentsManualPaymentPageResponse>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return user ?? throw new InvalidOperationException();
    }
}
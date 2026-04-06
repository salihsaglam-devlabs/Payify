using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;

public class HostedPaymentHttpClient : HttpClientBase, IHostedPaymentHttpClient
{
    public HostedPaymentHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task<HppTransactionResponse> GetHppTransactionByTrackingIdAsync(
        GetHppTransactionByTrackingIdRequest request)
    {
        var response = await GetAsync($"v1/HostedPayment/inquire?trackingId={request.TrackingId}&merchantId={request.MerchantId}");
        var hppTransactionResponse = await response.Content.ReadFromJsonAsync<HppTransactionResponse>();
        return hppTransactionResponse ?? throw new InvalidOperationException();
    }

    public async Task HostedPaymentWebhookTriggerAsync(string trackingId)
    {
        var response = await GetAsync($"v1/HostedPayment/webhook-trigger/{trackingId}");
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }
    }

    public async Task<PaginatedList<HostedPaymentDto>> GetHostedPaymentTransactionsAsync(
        GetHppTransactionsRequest request)
    {
        var url = CreateUrlWithParams($"v1/HostedPayment/transactions", request);

        var response = await GetAsync(url);

        var hppDetails = await response.Content.ReadFromJsonAsync<PaginatedList<HostedPaymentDto>>();

        return hppDetails ?? throw new InvalidOperationException();
    }
}
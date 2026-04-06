using LinkPara.ApiGateway.Services.Emoney.Models.Requests;
using LinkPara.HttpProviders.Emoney.Models;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Services.Emoney.HttpClients;

public class OnUsPaymentHttpClient : HttpClientBase, IOnUsPaymentHttpClient
{
    public OnUsPaymentHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task<OnUsPaymentResponse> InitOnUsPaymentAsync(InitOnUsRequest request)
    {
        var response = await PostAsJsonAsync($"v1/OnUsPayment/init", request);
        var onUsPaymentRequest = await response.Content.ReadFromJsonAsync<OnUsPaymentResponse>();

        return onUsPaymentRequest ?? throw new InvalidOperationException();
    }
    public async Task<OnUsPaymentRequest> GetOnUsPaymentDetailsAsync(Guid onUsPaymentRequestId)
    {
        var response = await GetAsync($"v1/OnUsPayment/details?onUsPaymentRequestId={onUsPaymentRequestId}");
        var onUsPaymentDetails = await response.Content.ReadFromJsonAsync<OnUsPaymentRequest>();

        return onUsPaymentDetails ?? throw new InvalidOperationException();
    }
    public async Task<ProvisionPreviewResponse> ApproveOnUsPaymentAsync([FromBody] OnUsPaymentApproveRequest request)
    {
        var response = await PutAsJsonAsync($"v1/OnUsPayment/approve", request);
        var approvedOnUsPayment = await response.Content.ReadFromJsonAsync<ProvisionPreviewResponse>();

        return approvedOnUsPayment ?? throw new InvalidOperationException();
    }
}
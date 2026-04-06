using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;

public interface IHostedPaymentHttpClient
{
    Task<HppTransactionResponse> GetHppTransactionByTrackingIdAsync(GetHppTransactionByTrackingIdRequest request);
    Task HostedPaymentWebhookTriggerAsync(string trackingId);
    Task<PaginatedList<HostedPaymentDto>> GetHostedPaymentTransactionsAsync(GetHppTransactionsRequest request);
}
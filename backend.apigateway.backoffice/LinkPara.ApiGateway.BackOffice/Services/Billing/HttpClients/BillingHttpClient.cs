using LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Billing.HttpClients;

public class BillingHttpClient : HttpClientBase, IBillingHttpClient
{
    public BillingHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) 
        : base(client, httpContextAccessor)
    {
    }

    public async Task<PaginatedList<BillTransactionDto>> GetBillTransactionsAsync(BillTransactionFilterRequest request)
    {
        var url = CreateUrlWithParams($"v1/Billings/bill-transactions", request, true);   
        var response = await GetAsync(url);

        return await response.Content.ReadFromJsonAsync<PaginatedList<BillTransactionDto>>();
    }

    public async Task<BillTransactionDto> GetBillInfoByConversationIdAsync(string conversationId)
    {
        var response = await GetAsync($"v1/Billings/bill-info/{conversationId}");

        return await response.Content.ReadFromJsonAsync<BillTransactionDto>();
    }

    public async Task<BillCancelResponseDto> CancelBillPaymentAsync(BillCancelRequest request)
    {
        var response = await PutAsJsonAsync($"v1/Billings/cancel-bill-payment", request);

        return await response.Content.ReadFromJsonAsync<BillCancelResponseDto>();
    }

    public async Task<BillCancelAccountingResponseDto> CancelBillAccountingAsync(BillCancelAccountingRequest request)
    {
        var response = await PutAsJsonAsync($"v1/Billings/cancel-bill-accounting", request);

        return await response.Content.ReadFromJsonAsync<BillCancelAccountingResponseDto>();
    }
}

using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;

public class MerchantTransactionHttpClient : HttpClientBase, IMerchantTransactionHttpClient
{
    public MerchantTransactionHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task<MerchantTransactionDto> GetByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/MerchantTransactions/{id}");
        var transaction = await response.Content.ReadFromJsonAsync<MerchantTransactionDto>();
        return transaction ?? throw new InvalidOperationException();
    }

    public async Task<PaginatedList<MerchantTransactionDto>> GetAllAsync(GetAllMerchantTransactionRequest request)
    {
        var url = CreateUrlWithParams($"v1/MerchantTransactions", request, true);
        var response = await GetAsync(url);
        var transactions = await response.Content.ReadFromJsonAsync<PaginatedList<MerchantTransactionDto>>();
        return transactions ?? throw new InvalidOperationException();
    }

    public async Task<List<MerchantTransactionStatusModel>> GetStatusCountAsync(MerchantTransactionStatusRequest request)
    {
        var url = CreateUrlWithParams($"v1/MerchantTransactions/statusCount", request, true);
        var response = await GetAsync(url);
        var transactions = await response.Content.ReadFromJsonAsync<List<MerchantTransactionStatusModel>>();
        return transactions ?? throw new InvalidOperationException();
    }
    public async Task<string> GenerateOrderNumberAsync(Guid merchantId)
    {
        var response = await GetAsync($"v1/MerchantTransactions/{merchantId}/generate-orderNumber");
        var transaction = await response.Content.ReadAsStringAsync();
        return transaction ?? throw new InvalidOperationException();
    }
    public async Task<OrderNumberResponse> GenerateUniqueOrderNumberAsync(OrderNumberRequest request)
    {
        var response = await PostAsJsonAsync($"v1/MerchantTransactions/generate-orderNumber", request);
        var orderNumber = await response.Content.ReadFromJsonAsync<OrderNumberResponse>();
        return orderNumber ?? throw new InvalidOperationException();
    }
}

using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public class MerchantBlockageHttpClient : HttpClientBase, IMerchantBlockageHttpClient
{
    public MerchantBlockageHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task<PaginatedList<MerchantBlockageDto>> GetAllAsync(GetFilterMerchantBlockageRequest request)
    {
        var url = CreateUrlWithParams($"v1/MerchantBlockage", request, true);
        var response = await GetAsync(url);
        var merchantBlockages = await response.Content.ReadFromJsonAsync<PaginatedList<MerchantBlockageDto>>();
        return merchantBlockages ?? throw new InvalidOperationException();
    }

    public async Task<MerchantBlockageDto> GetByMerchantIdAsync(Guid merchantId)
    {
        var response = await GetAsync($"v1/MerchantBlockage/{merchantId}");
        var merchantBlockage = await response.Content.ReadFromJsonAsync<MerchantBlockageDto>();
        return merchantBlockage ?? throw new InvalidOperationException();
    }

    public async Task SaveAsync(SaveMerchantBlockageRequest request)
    {
        var response = await PostAsJsonAsync($"v1/MerchantBlockage", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task UpdateAsync(UpdateMerchantBlockageRequest request)
    {
        var response = await PutAsJsonAsync($"v1/MerchantBlockage", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task UpdatePaymentDateAsync(UpdatePaymentDateRequest request)
    {
        var response = await PutAsJsonAsync($"v1/MerchantBlockage/payment-date", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }
}

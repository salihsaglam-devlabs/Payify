using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public class MerchantPreApplicationHttpClient : HttpClientBase, IMerchantPreApplicationHttpClient
{
    public MerchantPreApplicationHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) 
        : base(client, httpContextAccessor)
    {
    }

    public async Task<MerchantPreApplicationDto> GetByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/MerchantPreApplication/{id}");
        var merchantApplication = await response.Content.ReadFromJsonAsync<MerchantPreApplicationDto>();
        return merchantApplication ?? throw new InvalidOperationException();
    }

    public async Task<PaginatedList<MerchantPreApplicationDto>> GetAllAsync(GetAllMerchantPreApplicationsRequest request)
    {
        var url = CreateUrlWithParams($"v1/MerchantPreApplication", request, true);
        var response = await GetAsync(url);
        var merchantApplications = await response.Content.ReadFromJsonAsync<PaginatedList<MerchantPreApplicationDto>>();
        return merchantApplications ?? throw new InvalidOperationException();
    }

    public async Task UpdateAsync(UpdateMerchantPreApplicationRequest request)
    {
        var response = await PutAsJsonAsync($"v1/MerchantPreApplication", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task DeleteMerchantApplicationAsync(Guid id)
    {
        var response = await DeleteAsync($"v1/MerchantPreApplication/{id}");
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }
}
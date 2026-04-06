using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public class MerchantUserService : HttpClientBase, IMerchantUserService
{
    public MerchantUserService(HttpClient client, IHttpContextAccessor httpContextAccessor) 
        : base(client, httpContextAccessor)
    {
    }

    public async Task<PaginatedList<MerchantUserDto>> GetAllAsync(GetAllMerchantUserRequest request)
    {
        var url = CreateUrlWithParams($"v1/MerchantUsers", request, true);
        var response = await GetAsync(url);
        var merchantUsers = await response.Content.ReadFromJsonAsync<PaginatedList<MerchantUserDto>>();
        return merchantUsers ?? throw new InvalidOperationException();
    }

    public async Task<MerchantUserDto> GetByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/MerchantUsers/{id}");
        var merchantUser = await response.Content.ReadFromJsonAsync<MerchantUserDto>();
        return merchantUser ?? throw new InvalidOperationException();
    }

    public async Task SaveAsync(SaveMerchantUserRequest request)
    {
        var response = await PostAsJsonAsync($"v1/MerchantUsers", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task UpdateAsync(MerchantUserDto request)
    {
        var response = await PutAsJsonAsync($"v1/MerchantUsers", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }
}

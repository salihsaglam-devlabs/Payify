using LinkPara.ApiGateway.Merchant.Commons.Helpers;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;

public class SubMerchantUserHttpClient : HttpClientBase, ISubMerchantUserHttpClient
{
    public SubMerchantUserHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) 
        : base(client, httpContextAccessor)
    {
    }

    public async Task<PaginatedList<SubMerchantUserDto>> GetAllAsync(GetAllSubMerchantUserRequest request)
    {
        var url = CreateUrlWithParams($"v1/SubMerchantUsers", request, true);
        var response = await GetAsync(url);
        var merchantUsers = await response.Content.ReadFromJsonAsync<PaginatedList<SubMerchantUserDto>>();
        if (!CanSeeSensitiveData())
        {
            merchantUsers.Items.ForEach(m =>
            {
                m.IdentityNumber = SensitiveDataHelper.MaskSensitiveData("IdentityNumber", m.IdentityNumber);
            });

        }
        return merchantUsers ?? throw new InvalidOperationException();
    }

    public async Task<SubMerchantUserDto> GetByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/SubMerchantUsers/{id}");
        var merchantUser = await response.Content.ReadFromJsonAsync<SubMerchantUserDto>();
        return merchantUser ?? throw new InvalidOperationException();
    }

    public async Task SaveAsync(SaveSubMerchantUserRequest request)
    {
        var response = await PostAsJsonAsync($"v1/SubMerchantUsers", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task UpdateAsync(SubMerchantUserDto request)
    {
        var response = await PutAsJsonAsync($"v1/SubMerchantUsers", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }
}

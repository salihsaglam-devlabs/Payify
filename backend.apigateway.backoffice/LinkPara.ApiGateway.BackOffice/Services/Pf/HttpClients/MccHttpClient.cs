using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.JsonPatch;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public class MccHttpClient : HttpClientBase, IMccHttpClient
{
    public MccHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) : 
        base(client, httpContextAccessor)
    {
    }

    public async Task DeleteMccAsync(Guid id)
    {
        var response = await DeleteAsync($"v1/MerchantCategoryCodes/{id}");
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task<PaginatedList<MccDto>> GetAllAsync(GetFilterMccRequest request)
    {
        var url = CreateUrlWithParams($"v1/MerchantCategoryCodes", request, true);
        var response = await GetAsync(url);
        var mccList = await response.Content.ReadFromJsonAsync<PaginatedList<MccDto>>();
        return mccList ?? throw new InvalidOperationException();
    }

    public async Task<MccDto> GetByCodeAsync(string code)
    {
        var response = await GetAsync($"v1/MerchantCategoryCodes/{code}");
        var mcc = await response.Content.ReadFromJsonAsync<MccDto>();
        return mcc ?? throw new InvalidOperationException();
    }

    public async Task<MccDto> GetByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/MerchantCategoryCodes/{id}");
        var mcc = await response.Content.ReadFromJsonAsync<MccDto>();
        return mcc ?? throw new InvalidOperationException();
    }

    public async Task<UpdateMccRequest> PatchAsync(Guid id, JsonPatchDocument<UpdateMccRequest> mccPatch)
    {
        var response = await PatchAsync($"v1/MerchantCategoryCodes/{id}", mccPatch);
        var mcc = await response.Content.ReadFromJsonAsync<UpdateMccRequest>();
        return mcc ?? throw new InvalidOperationException();
    }

    public async Task SaveAsync(SaveMccRequest request)
    {
        var response = await PostAsJsonAsync($"v1/MerchantCategoryCodes", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task UpdateAsync(UpdateMccRequest request)
    {
        var response = await PutAsJsonAsync($"v1/MerchantCategoryCodes", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }
}

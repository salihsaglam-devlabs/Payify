using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.JsonPatch;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public class AcquireBankHttpClient : HttpClientBase, IAcquireBankHttpClient
{
    public AcquireBankHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) 
        : base(client, httpContextAccessor)
    {
    }

    public async Task<PaginatedList<AcquireBankDto>> GetAllAsync(GetFilterAcquireBankRequest request)
    {
        var url = CreateUrlWithParams($"v1/AcquireBanks", request, true);
        var response = await GetAsync(url);
        var acquireBanks = await response.Content.ReadFromJsonAsync<PaginatedList<AcquireBankDto>>();
        return acquireBanks ?? throw new InvalidOperationException();
    }

    public async Task<AcquireBankDto> GetByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/AcquireBanks/{id}");
        var acquireBank = await response.Content.ReadFromJsonAsync<AcquireBankDto>();
        return acquireBank ?? throw new InvalidOperationException();
    }

    public async Task DeleteAcquireBankAsync(Guid id)
    {
        var response = await DeleteAsync($"v1/AcquireBanks/{id}");
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task SaveAsync(SaveAcquireBankRequest request)
    {
        var response = await PostAsJsonAsync($"v1/AcquireBanks", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task UpdateAsync(UpdateAcquireBankRequest request)
    {
        var response = await PutAsJsonAsync($"v1/AcquireBanks", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task<UpdateAcquireBankRequest> PatchAsync(Guid id, JsonPatchDocument<UpdateAcquireBankRequest> acquireBank)
    {
        var response = await PatchAsync($"v1/AcquireBanks/{id}", acquireBank);
        var acquireBankPatch = await response.Content.ReadFromJsonAsync<UpdateAcquireBankRequest>();
        return acquireBankPatch ?? throw new InvalidOperationException();
    }
}

using DocumentFormat.OpenXml.Office2010.Excel;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.JsonPatch;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public class VposHttpClient : HttpClientBase, IVposHttpClient
{
    public VposHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
    {
    }

    public async Task DeleteVposAsync(Guid id)
    {
        var response = await DeleteAsync($"v1/Vpos/{id}");
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task<VposDto> GetByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/Vpos/{id}");
        var vpos = await response.Content.ReadFromJsonAsync<VposDto>();
        return vpos ?? throw new InvalidOperationException();
    }

    public async Task<MerchantVposDto> GetByReferenceNumberAsync(string bkmReferenceNumber)
    {
        var response = await GetAsync($"v1/Vpos/bkm/{bkmReferenceNumber}");
        var vpos = await response.Content.ReadFromJsonAsync<MerchantVposDto>();
        return vpos ?? throw new InvalidOperationException();
    }

    public async Task<PaginatedList<VposDto>> GetFilterListAsync(GetFilterVposRequest request)
    {
        var url = CreateUrlWithParams($"v1/Vpos", request, true);
        var response = await GetAsync(url);
        var vpos = await response.Content.ReadFromJsonAsync<PaginatedList<VposDto>>();
        return vpos ?? throw new InvalidOperationException();
    }

    public async Task<PatchVposRequest> PatchAsync(Guid id, JsonPatchDocument<PatchVposRequest> vposPatch)
    {
        var response = await PatchAsync($"v1/Vpos/{id}", vposPatch);
        var vpos = await response.Content.ReadFromJsonAsync<PatchVposRequest>();
        return vpos ?? throw new InvalidOperationException();
    }

    public async Task SaveAsync(SaveVposRequest request)
    {
        var response = await PostAsJsonAsync($"v1/Vpos", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task UpdateAsync(UpdateVposRequest request)
    {
        var response = await PutAsJsonAsync($"v1/Vpos", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }
}

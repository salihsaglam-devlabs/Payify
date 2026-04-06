using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests.PhysicalPos;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses.PhysicalPos;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients.PhysicalPos;

public class PhysicalPosHttpClient : HttpClientBase, IPhysicalPosHttpClient
{
    public PhysicalPosHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
    {
    }

    public async Task DeletePhysicalPosAsync(Guid id)
    {
        var response = await DeleteAsync($"v1/PhysicalPos/{id}");
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task<PaginatedList<PhysicalPosDto>> GetAllAsync(GetAllPhysicalPosRequest request)
    {
        var url = CreateUrlWithParams($"v1/PhysicalPos", request, true);
        var response = await GetAsync(url);
        var physicalPosList = await response.Content.ReadFromJsonAsync<PaginatedList<PhysicalPosDto>>();
        return physicalPosList ?? throw new InvalidOperationException();
    }

    public async Task<PhysicalPosDto> GetByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/PhysicalPos/{id}");
        var physicalPos = await response.Content.ReadFromJsonAsync<PhysicalPosDto>();
        return physicalPos ?? throw new InvalidOperationException();
    }

    public async Task<MerchantPhysicalPosDto> GetByReferenceNumberAsync(string bkmReferenceNumber)
    {
        var response = await GetAsync($"v1/PhysicalPos/bkm/{bkmReferenceNumber}");
        var merchantPyhsicalPos = await response.Content.ReadFromJsonAsync<MerchantPhysicalPosDto>();
        return merchantPyhsicalPos ?? throw new InvalidOperationException();
    }

    public async Task SaveAsync(SavePhysicalPosRequest request)
    {
        var response = await PostAsJsonAsync($"v1/PhysicalPos", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task UpdateAsync(UpdatePhysicalPosRequest request)
    {
        var response = await PutAsJsonAsync($"v1/PhysicalPos", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }
}

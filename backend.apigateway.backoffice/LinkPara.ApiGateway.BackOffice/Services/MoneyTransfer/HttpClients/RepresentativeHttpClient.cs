using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests.Representative;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.JsonPatch;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;

public class RepresentativeHttpClient : HttpClientBase, IRepresentativeHttpClient
{
    public RepresentativeHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        :base(client, httpContextAccessor)
    {

    }

    public async Task<PaginatedList<RepresentativeDto>> GetListAsync(GetRepresentativesRequest request)
    {
        var url = CreateUrlWithParams("v1/Representatives", request, true);
        var response = await GetAsync(url);
        
        return await response.Content.ReadFromJsonAsync<PaginatedList<RepresentativeDto>>();
    }

    public async Task SaveAsync(SaveRepresentativeRequest request)
    {
        await PostAsJsonAsync("v1/Representatives", request);
    }

    public async Task PatchAsync(Guid id, JsonPatchDocument<UpdateRepresentativeRequest> request)
    {
        var response = await PatchAsync($"v1/Representatives/{id}", request);
        
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }
    }
}
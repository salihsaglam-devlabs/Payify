using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests.Branch;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.JsonPatch;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;

public class BranchHttpClient : HttpClientBase, IBranchHttpClient
{
    public BranchHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {

    }

    public async Task<PaginatedList<BranchDto>> GetListAsync(GetBranchesRequest request)
    {
        var url = CreateUrlWithParams("v1/Branches", request, true);
        var response = await GetAsync(url);

        return await response.Content.ReadFromJsonAsync<PaginatedList<BranchDto>>();
    }

    public async Task SaveAsync(SaveBranchRequest request)
    {
        await PostAsJsonAsync("v1/Branches", request);
    }

    public async Task PatchAsync(Guid id, JsonPatchDocument<UpdateBranchRequest> request)
    {
        var response = await PatchAsync($"v1/Branches/{id}", request);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }
    }
}
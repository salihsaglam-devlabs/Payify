using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;

public class PostingBalanceHttpClient : HttpClientBase, IPostingBalanceHttpClient
{
    public PostingBalanceHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task<ActionResult<PostingBalanceResponse>> GetAllAsync(GetAllPostingBalanceRequest request)
    {
        var url = CreateUrlWithParams($"v1/PostingBalances", request, true);
        var response = await GetAsync(url);
        var postingBalances = await response.Content.ReadFromJsonAsync<PostingBalanceResponse>();
        return postingBalances ?? throw new InvalidOperationException();
    }

    public async Task<ActionResult<PostingBalanceDto>> GetByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/PostingBalances/{id}");
        var postingBalance = await response.Content.ReadFromJsonAsync<PostingBalanceDto>();
        return postingBalance ?? throw new InvalidOperationException();
    }

    public async Task<List<PostingBalanceStatusModel>> GetStatusCountAsync(PostingBalanceStatusRequest request)
    {
        var url = CreateUrlWithParams($"v1/PostingBalances/statusCount", request, true);
        var response = await GetAsync(url);
        var postingBalances = await response.Content.ReadFromJsonAsync<List<PostingBalanceStatusModel>>();
        return postingBalances ?? throw new InvalidOperationException();
    }

    public async Task PatchAsync(Guid id, JsonPatchDocument<PatchPostingBalanceRequest> request)
    {
        var response = await PatchAsync($"v1/PostingBalances/{id}", request);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }
    }
}

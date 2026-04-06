using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

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

    public async Task RetryPostingPaymentAsync(RetryPostingPaymentRequest request)
    {
        var response = await PutAsJsonAsync($"v1/PostingBalances/retry-payment", request);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }
    }
    
    public async Task<ActionResult<PostingBalanceStatisticsResponse>> GetStatisticsAsync(PostingBalanceStatisticsRequest request)
    {
        var url = CreateUrlWithProperties($"v1/PostingBalances/statistics", request);
        var response = await GetAsync(url);
        var postingBalanceStatistics = await response.Content.ReadFromJsonAsync<PostingBalanceStatisticsResponse>();
        return postingBalanceStatistics ?? throw new InvalidOperationException();
    }

    public async Task<ActionResult<PaginatedList<PostingPfProfitDto>>> GetAllPostingPfProfits(GetAllPostingPfProfitsRequest request)
    {
        var url = CreateUrlWithProperties($"v1/PostingBalances/pf-profits", request);
        var response = await GetAsync(url);
        var postingPfProfits = await response.Content.ReadFromJsonAsync<PaginatedList<PostingPfProfitDto>>();
        return postingPfProfits ?? throw new InvalidOperationException();
    }
}

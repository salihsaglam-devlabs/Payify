using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public class PostingHttpClient : HttpClientBase, IPostingHttpClient
{
    public PostingHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task<PaginatedList<PostingTransferErrorDto>> GetAllTransferErrorAsync(GetAllPostingTransferErrorRequest request)
    {
        var url = CreateUrlWithParams($"v1/Posting/transfer-error", request, true);
        var response = await GetAsync(url);
        var postingBalances = await response.Content.ReadFromJsonAsync<PaginatedList<PostingTransferErrorDto>>();
        return postingBalances ?? throw new InvalidOperationException();
    }

    public async Task<PaginatedList<PostingBillDto>> GetBillsAsync(GetPostingBillRequest request)
    {
        var url = CreateUrlWithParams($"v1/Posting/bills", request, true);
        var response = await GetAsync(url);
        var postingBills = await response.Content.ReadFromJsonAsync<PaginatedList<PostingBillDto>>();

        return postingBills ?? throw new InvalidOperationException();
    }
}
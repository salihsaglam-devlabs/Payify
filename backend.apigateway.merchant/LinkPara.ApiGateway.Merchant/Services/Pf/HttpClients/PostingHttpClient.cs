using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;

public class PostingHttpClient : HttpClientBase, IPostingHttpClient
{
    public PostingHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) : 
        base(client, httpContextAccessor)
    {
    }

    public async Task<PaginatedList<PostingBillDto>> GetBillsAsync(Guid merchantId, GetPostingBillRequest request)
    {
        request.MerchantId = merchantId;
        var url = CreateUrlWithParams($"v1/Posting/bills", request, true);
        var response = await GetAsync(url);
        var postingBills = await response.Content.ReadFromJsonAsync<PaginatedList<PostingBillDto>>();

        return postingBills ?? throw new InvalidOperationException();
    }
}

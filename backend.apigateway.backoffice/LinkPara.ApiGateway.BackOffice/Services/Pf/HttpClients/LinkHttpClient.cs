using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public class LinkHttpClient : HttpClientBase, ILinkHttpClient
{
    public LinkHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
         : base(client, httpContextAccessor)
    {
    }

    public async Task<PaginatedList<LinkDto>> GetAllAsync(GetFilterLinkRequest request)
    {
        var response = await PostAsJsonAsync($"v1/Link/payment-report", request);
        var linkList = await response.Content.ReadFromJsonAsync<PaginatedList<LinkDto>>();
        return linkList ?? throw new InvalidOperationException();
    }
    public async Task<PaginatedList<LinkPaymentDetailResponse>> GetLinkPaymentDetailAsync(GetPaymentDetailRequest request)
    {
        var url = CreateUrlWithProperties("v1/LinkPayment/detail", request);
        var response = await GetAsync(url);
        var linkPaymentDetails = await response.Content.ReadFromJsonAsync<PaginatedList<LinkPaymentDetailResponse>>();
        return linkPaymentDetails ?? throw new InvalidOperationException();
    }
}

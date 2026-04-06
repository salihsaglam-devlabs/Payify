using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients
{
    public class LinkHttpClient : HttpClientBase, ILinkHttpClient
    {
        public LinkHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
             : base(client, httpContextAccessor)
        {
        }
        public async Task<LinkResponse> SaveAsync(SaveLinkRequest request)
        {
            var response  = await PostAsJsonAsync($"v1/Link", request);
            var link = await response.Content.ReadFromJsonAsync<LinkResponse>();
            return link ?? throw new InvalidOperationException();
        }
        public async Task<LinkRequirementResponse> GetCreateLinkRequirements(Guid merchantId)
        {
            var response = await GetAsync($"v1/Link/requirements/{merchantId}");
            var linkRequirementResponse = await response.Content.ReadFromJsonAsync<LinkRequirementResponse>();
            return linkRequirementResponse ?? throw new InvalidOperationException();
        }
        public async Task DeleteLinkAsync(Guid id)
        {
            var response = await DeleteAsync($"v1/Link/{id}");
            if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
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
}

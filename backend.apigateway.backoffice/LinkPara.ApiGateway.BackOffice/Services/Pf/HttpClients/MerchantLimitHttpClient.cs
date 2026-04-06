using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients
{
    public class MerchantLimitHttpClient : HttpClientBase, IMerchantLimitHttpClient
    {
        public MerchantLimitHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
                                       : base(client, httpContextAccessor)
        {
            
        }
        public async Task<PaginatedList<MerchantLimitDto>> GetFilterListAsync(GetFilterMerchantLimitRequest request)
        {
            var url = CreateUrlWithParams($"v1/MerchantLimits", request, true);
            var response = await GetAsync(url);
            var merchantLimits = await response.Content.ReadFromJsonAsync<PaginatedList<MerchantLimitDto>>();
            return merchantLimits ?? throw new InvalidOperationException();
        }

        public async Task SaveAsync(SaveMerchantLimitRequest request)
        {
            var response = await PostAsJsonAsync($"v1/MerchantLimits", request);
            if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
        }

        public async Task UpdateAsync(UpdateMerchantLimitRequest request)
        {
            var response = await PutAsJsonAsync($"v1/MerchantLimits", request);
            if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
        }

    }
}

using System.Net.Http.Json;
using LinkPara.HttpProviders.PF.Models.Request;
using LinkPara.HttpProviders.PF.Models.Response;
using LinkPara.HttpProviders.Utility;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Http;


namespace LinkPara.HttpProviders.PF
{
    public class MerchantService : HttpClientBase, IMerchantService
    {
        public MerchantService(HttpClient client, IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
        {
        }

        public async Task UpdateMerchantIKSAsync(UpdateMerchantIKSModel request)
        {
            var response = await PutAsJsonAsync("v1/Merchants/updateMerchantIKS", request);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException();
            }
        }

        public async Task<PaginatedList<MerchantDto>> GetFilterListAsync(GetFilterMerchantsRequest request)
        {
            var url = GetQueryString.CreateUrlWithSearchQueryParams($"v1/Merchants", request, true);

            var response = await GetAsync(url);

            var merchants = await response.Content.ReadFromJsonAsync<PaginatedList<MerchantDto>>();

            return merchants ?? throw new InvalidOperationException();
        }
    }
}

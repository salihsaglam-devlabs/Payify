using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients
{
    public class MerchantBusinessPartnerHttpClient : HttpClientBase, IMerchantBusinessPartnerHttpClient
    {
        public MerchantBusinessPartnerHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
            : base(client, httpContextAccessor)
        {
        }

        public async Task<PaginatedList<MerchantBusinessPartnerDto>> GetAllAsync(GetAllMerchantBusinessPartnerRequest request)
        {
            var url = CreateUrlWithParams($"v1/MerchantBusinessPartner", request, true);
            var response = await GetAsync(url);
            var merchantBusinessPartners = await response.Content.ReadFromJsonAsync<PaginatedList<MerchantBusinessPartnerDto>>();
            return merchantBusinessPartners ?? throw new InvalidOperationException();
        }
        public async Task<MerchantBusinessPartnerDto> GetByIdAsync(Guid id)
        {
            var response = await GetAsync($"v1/MerchantBusinessPartner/{id}");
            var merchantBusinessPartner = await response.Content.ReadFromJsonAsync<MerchantBusinessPartnerDto>();
            return merchantBusinessPartner ?? throw new InvalidOperationException();
        }

        public async Task SaveAsync(SaveMerchantBusinessPartnerRequest request)
        {
            var response = await PostAsJsonAsync($"v1/MerchantBusinessPartner", request);
            if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
        }

        public async Task UpdateAsync(MerchantBusinessPartnerDto request)
        {
            var response = await PutAsJsonAsync($"v1/MerchantBusinessPartner", request);
            if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
        }

    }
}

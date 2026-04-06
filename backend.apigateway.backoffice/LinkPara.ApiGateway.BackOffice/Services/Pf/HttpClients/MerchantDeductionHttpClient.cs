using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients
{
    public class MerchantDeductionHttpClient : HttpClientBase, IMerchantDeductionHttpClient
    {
        public MerchantDeductionHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) 
            : base(client, httpContextAccessor)
        {
        }

        public async Task<PaginatedList<MerchantDeductionDto>> GetAllMerchantDeductionsAsync(GetFilterMerchantDeductionRequest request)
        {
            var url = CreateUrlWithParams($"v1/MerchantDeduction", request, true);
            var response = await GetAsync(url);
            var merchantDeductions = await response.Content.ReadFromJsonAsync<PaginatedList<MerchantDeductionDto>>();
            return merchantDeductions ?? throw new InvalidOperationException();
        }

        public async Task<DeductionDetailsResponse> GetByIdAsync(Guid id)
        {
            var response = await GetAsync($"v1/MerchantDeduction/{id}");
            var merchantDeduction = await response.Content.ReadFromJsonAsync<DeductionDetailsResponse>();
            return merchantDeduction ?? throw new InvalidOperationException();
        }
    }
}

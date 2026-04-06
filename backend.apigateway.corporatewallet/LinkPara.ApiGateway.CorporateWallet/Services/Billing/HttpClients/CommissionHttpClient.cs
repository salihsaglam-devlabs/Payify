using LinkPara.ApiGateway.CorporateWallet.Commons.Extensions;
using LinkPara.ApiGateway.CorporateWallet.Services.Billing.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.Billing.Models.Responses;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Billing.HttpClients
{
    public class CommissionHttpClient : HttpClientBase, ICommissionHttpClient
    {
        public CommissionHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) 
            : base(client, httpContextAccessor)
        {
        }

        public async Task<CommissionDto> GetByDetailAsync(CommissionFilterRequest request)
        {
            var queryString = request.GetQueryString();
            var response = await GetAsync($"v1/Commissions/by-detail" + queryString);

            return await response.Content.ReadFromJsonAsync<CommissionDto>();
        }
    }
}

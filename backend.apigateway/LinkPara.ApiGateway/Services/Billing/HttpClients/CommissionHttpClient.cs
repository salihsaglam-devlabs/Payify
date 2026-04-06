using LinkPara.ApiGateway.Commons.Extensions;
using LinkPara.ApiGateway.Services.Billing.Models.Requests;
using LinkPara.ApiGateway.Services.Billing.Models.Responses;

namespace LinkPara.ApiGateway.Services.Billing.HttpClients
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

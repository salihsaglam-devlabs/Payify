using LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Responses;

namespace LinkPara.ApiGateway.BackOffice.Services.Billing.HttpClients;

public class BillingStatisticHttpClient : HttpClientBase, IBillingStatisticHttpClient
{
    public BillingStatisticHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) 
        : base(client, httpContextAccessor)
    {
    }

    public async Task<BillingReportDto> GetReportAsync(DateTime startDate, DateTime endDate)
    {
        var sDate = $"{startDate:yyyy-MM-ddTHH:mm:ss}";
        var eDate = $"{endDate:yyyy-MM-ddTHH:mm:ss}";

        var response = await GetAsync($"v1/Statistics?startDate={sDate}&endDate={eDate}");
        return await response.Content.ReadFromJsonAsync<BillingReportDto>();
    }
}

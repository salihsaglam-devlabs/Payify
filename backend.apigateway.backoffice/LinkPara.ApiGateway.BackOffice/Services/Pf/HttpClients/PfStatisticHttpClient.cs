using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public class PfStatisticHttpClient : HttpClientBase, IPfStatisticHttpClient
{
    public PfStatisticHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) 
        : base(client, httpContextAccessor)
    {
    }

    public async Task<PfReportDto> GetReportAsync(DateTime startDate, DateTime endDate)
    {
        var sDate = $"{startDate:yyyy-MM-ddTHH:mm:ss}";
        var eDate = $"{endDate:yyyy-MM-ddTHH:mm:ss}";

        var response = await GetAsync($"v1/Statistics?startDate={sDate}&endDate={eDate}");
        return await response.Content.ReadFromJsonAsync<PfReportDto>();
    }
}

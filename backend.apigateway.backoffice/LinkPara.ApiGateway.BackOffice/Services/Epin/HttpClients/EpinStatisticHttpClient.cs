using LinkPara.ApiGateway.BackOffice.Services.Epin.Models.Responses;

namespace LinkPara.ApiGateway.BackOffice.Services.Epin.HttpClients;

public class EpinStatisticHttpClient : HttpClientBase, IEpinStatisticHttpClient
{
    public EpinStatisticHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) 
        : base(client, httpContextAccessor)
    {
    }

    public async Task<EpinReportDto> GetReportAsync(DateTime startDate, DateTime endDate)
    {
        var sDate = $"{startDate:yyyy-MM-ddTHH:mm:ss}";
        var eDate = $"{endDate:yyyy-MM-ddTHH:mm:ss}";

        var response = await GetAsync($"v1/Statistics?startDate={sDate}&endDate={eDate}");
        return await response.Content.ReadFromJsonAsync<EpinReportDto>();
    }
}

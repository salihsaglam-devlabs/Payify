using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;

public class EmoneyStatisticHttpClient : HttpClientBase, IEmoneyStatisticHttpClient
{
    public EmoneyStatisticHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task<EmoneyReportDto> GetReportAsync(DateTime startDate, DateTime endDate)
    {
        var sDate = $"{startDate:yyyy-MM-ddTHH:mm:ss}";
        var eDate = $"{endDate:yyyy-MM-ddTHH:mm:ss}";

        var response = await GetAsync($"v1/Statistics?startDate={sDate}&endDate={eDate}");
        return await response.Content.ReadFromJsonAsync<EmoneyReportDto>();
    }
}
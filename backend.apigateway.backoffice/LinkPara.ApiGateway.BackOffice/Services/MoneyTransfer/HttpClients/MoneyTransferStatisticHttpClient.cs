using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;

public class MoneyTransferStatisticHttpClient : HttpClientBase, IMoneyTransferStatisticHttpClient
{
    public MoneyTransferStatisticHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task<MoneyTransferReportDto> GetReportAsync(DateTime startDate, DateTime endDate)
    {
        var sDate = $"{startDate:yyyy-MM-ddTHH:mm:ss}";
        var eDate = $"{endDate:yyyy-MM-ddTHH:mm:ss}";

        var response = await GetAsync($"v1/Statistics?startDate={sDate}&endDate={eDate}");
        return await response.Content.ReadFromJsonAsync<MoneyTransferReportDto>();
    }
}

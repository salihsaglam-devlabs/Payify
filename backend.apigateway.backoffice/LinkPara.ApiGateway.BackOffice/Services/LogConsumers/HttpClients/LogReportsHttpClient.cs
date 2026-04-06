using LinkPara.ApiGateway.BackOffice.Services.LogConsumers.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.LogConsumers.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.LogConsumers.HttpClients;

public class LogReportsHttpClient : HttpClientBase, ILogReportsHtppClient
{
    public LogReportsHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }
    public async Task<PaginatedList<EntityChangeLogDto>> GetEntityChangeLogsAsync(GetFilterEntityChangeLogRequest request)
    {
        var url = CreateUrlWithParams($"v1/LogReports/entity-change-log", request, true);
        var response = await GetAsync(url);
        var logs = await response.Content.ReadFromJsonAsync<PaginatedList<EntityChangeLogDto>>();
        return logs ?? throw new InvalidOperationException();
    }
}

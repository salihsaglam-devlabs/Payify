using LinkPara.ApiGateway.BackOffice.Services.LogConsumers.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.LogConsumers.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.LogConsumers.HttpClients;

public interface ILogReportsHtppClient
{
    Task<PaginatedList<EntityChangeLogDto>> GetEntityChangeLogsAsync(GetFilterEntityChangeLogRequest request);
}

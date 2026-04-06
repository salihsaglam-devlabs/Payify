using LinkPara.ApiGateway.BackOffice.Services.IKS.Models.Request;
using LinkPara.ApiGateway.BackOffice.Services.IKS.Models.Response;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.IKS.HttpClients;

public interface ITerminalHistoryHttpClient
{
    Task<PaginatedList<IksTerminalHistoryDto>> GetAllTerminalHistoryAsync(GetAllTerminalHistoryRequest request);
    Task<PaginatedList<IksTerminalDto>> GetAllTerminalAsync(GetAllTerminalRequest request);
}

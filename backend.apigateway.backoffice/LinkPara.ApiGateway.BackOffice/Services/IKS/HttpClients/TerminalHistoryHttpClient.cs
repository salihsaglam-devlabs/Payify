
using LinkPara.ApiGateway.BackOffice.Services.IKS.Models.Request;
using LinkPara.ApiGateway.BackOffice.Services.IKS.Models.Response;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.IKS.HttpClients;

public class TerminalHistoryHttpClient : HttpClientBase, ITerminalHistoryHttpClient
{
    public TerminalHistoryHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
    {
    }

    public async Task<PaginatedList<IksTerminalDto>> GetAllTerminalAsync(GetAllTerminalRequest request)
    {
        var url = CreateUrlWithParams($"v1/Terminals/get-terminals", request, true);
        var response = await GetAsync(url);
        var terminals = await response.Content.ReadFromJsonAsync<PaginatedList<IksTerminalDto>>();
        return terminals ?? throw new InvalidOperationException();
    }

    public async Task<PaginatedList<IksTerminalHistoryDto>> GetAllTerminalHistoryAsync(GetAllTerminalHistoryRequest request)
    {
        var url = CreateUrlWithParams($"v1/Terminals/get-terminal-histories", request, true);
        var response = await GetAsync(url);
        var terminalHistories = await response.Content.ReadFromJsonAsync<PaginatedList<IksTerminalHistoryDto>>();
        return terminalHistories ?? throw new InvalidOperationException();
    }
}

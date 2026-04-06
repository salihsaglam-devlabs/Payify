using LinkPara.ApiGateway.BackOffice.Services.IKS.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.IKS.Models.Request;
using LinkPara.ApiGateway.BackOffice.Services.IKS.Models.Response;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.IKS;

public class TerminalHistoryController : ApiControllerBase
{
    private readonly ITerminalHistoryHttpClient _terminalHistoryHttpClient;
    public TerminalHistoryController(ITerminalHistoryHttpClient terminalHistoryHttpClient)
    {
        _terminalHistoryHttpClient = terminalHistoryHttpClient;
    }

    /// <summary>
    /// Returns all terminal histories.
    /// </summary>
    /// <returns></returns>
    [HttpGet("get-terminal-histories")]
    [Authorize(Policy = "MerchantIksTerminal:ReadAll")]
    public async Task<ActionResult<PaginatedList<IksTerminalHistoryDto>>> GetAllAsync([FromQuery] GetAllTerminalHistoryRequest request)
    {
        return await _terminalHistoryHttpClient.GetAllTerminalHistoryAsync(request);
    }

    /// <summary>
    /// Returns all terminal histories.
    /// </summary>
    /// <returns></returns>
    [HttpGet("get-terminals")]
    [Authorize(Policy = "MerchantIksTerminal:ReadAll")]
    public async Task<ActionResult<PaginatedList<IksTerminalDto>>> GetAllAsync([FromQuery] GetAllTerminalRequest request)
    {
        return await _terminalHistoryHttpClient.GetAllTerminalAsync(request);
    }
}

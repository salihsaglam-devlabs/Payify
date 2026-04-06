using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.MoneyTransfer;
public class OperationalTransferReportUsersController : ApiControllerBase
{
    private readonly IOperationalTransferReportUserHttpClient _client;
    public OperationalTransferReportUsersController(IOperationalTransferReportUserHttpClient client)
    {
        _client = client;
    }
    /// <summary>
    /// Returns operational transfer report users.
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "OperationalTransferReportUser:ReadAll")]
    [HttpGet("")]
    public async Task<List<OperationalTransferReportUserDto>> GetListAsync()
    {
        return await _client.GetListAsync();
    }

    /// <summary>
    /// Syncs operational transfer report user.
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "OperationalTransferReportUser:Create")]
    [HttpPost("")]
    public async Task SyncAsync(SyncOperationalTransferReportUserRequest request)
    {
        await _client.SyncAsync(request);
    }
}

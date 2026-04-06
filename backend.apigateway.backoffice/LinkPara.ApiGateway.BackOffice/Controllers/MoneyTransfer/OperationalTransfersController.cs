using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.MoneyTransfer;

public class OperationalTransfersController : ApiControllerBase
{
    private readonly IOperationalTransferHttpClient _httpClient;

    public OperationalTransfersController(IOperationalTransferHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Return operational transfer balance.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "OperationalTransfer:Read")]
    [HttpGet("{id}")]
    public async Task<OperationalTransferDto> GetByIdAsync(Guid id)
    {
        return await _httpClient.GetOperationalTransferByIdAsync(id);
    }

    /// <summary>
    /// Returns operational transfer balances.
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "OperationalTransfer:ReadAll")]
    [HttpGet("")]
    public async Task<PaginatedList<OperationalTransferDto>> GetListAsync([FromQuery] GetOperationalTransferListRequest request)
    {
        return await _httpClient.GetOperationalTransferListAsync(request);
    }
}

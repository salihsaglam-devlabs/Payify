using LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Emoney;

public class TransferOrdersController : ApiControllerBase
{
    private readonly ITransferOrderHttpClient _transferOrderHttpClient;

    public TransferOrdersController(ITransferOrderHttpClient transferOrderHttpClient)
    {
        _transferOrderHttpClient = transferOrderHttpClient;
    }

    /// <summary>
    /// Returns all transfer orders summary of the user as a list.
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyTransferOrder:ReadAll")]
    [HttpGet("")]
    public async Task<ActionResult<PaginatedList<TransferOrderDto>>> GetTransferOrdersByUserIdAsync([FromQuery] GetTransferOrdersRequest query)
    {
        return await _transferOrderHttpClient.GetTransferOrdersByUserIdAsync(query);
    }

    /// <summary>
    /// Returns a transfer orders detail of the user.
    /// </summary>
    /// <param name="transferOrderId"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyTransferOrder:Read")]
    [HttpGet("{transferOrderId}")]
    public async Task<ActionResult<TransferOrderDto>> GetTransferOrderByIdAsync(Guid transferOrderId)
    {
        return await _transferOrderHttpClient.GetTransferOrderByIdAsync(transferOrderId);
    }
}

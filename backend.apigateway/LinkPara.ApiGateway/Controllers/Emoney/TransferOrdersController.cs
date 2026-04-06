using LinkPara.ApiGateway.Services.Emoney.HttpClients;
using LinkPara.ApiGateway.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Services.Emoney.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Controllers.Emoney;

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
    [Authorize(Policy = "EmoneyTransferOrder:ReadAll")]
    [HttpGet("")]
    public async Task<ActionResult<PaginatedList<TransferOrderDto>>> GetTransferOrdersByUserIdAsync([FromQuery] GetTransferOrdersRequest query)
    {
        return await _transferOrderHttpClient.GetTransferOrdersByUserIdAsync(query);
    }

    /// <summary>
    /// Returns a transfer orders detail of the user.
    /// </summary>
    /// <param name="transferOrderId">Transfer Order id</param>
    [Authorize(Policy = "EmoneyTransferOrder:Read")]
    [HttpGet("{transferOrderId}")]
    public async Task<ActionResult<TransferOrderDto>> GetTransferOrderByIdAsync(Guid transferOrderId)
    {
        return await _transferOrderHttpClient.GetTransferOrderByIdAsync(transferOrderId);
    }

    /// <summary>
    /// Creates a new transfer order for the user
    /// </summary>
    /// <param name="request"></param>
    [Authorize(Policy = "EmoneyTransferOrder:Create")]
    [HttpPost("")]
    public async Task CreateTransferOrderAsync(CreateTransferOrderRequest request)
    {
        await _transferOrderHttpClient.CreateTransferOrderAsync(request);
    }

    /// <summary>
    /// Updates the transfer order that the user has created before and has not yet been processed.
    /// </summary>
    /// <param name="transferOrderId"></param>
    /// <param name="request"></param>
    [Authorize(Policy = "EmoneyTransferOrder:Update")]
    [HttpPut("{transferOrderId}")]
    public async Task UpdateTransferOrderAsync(Guid transferOrderId, UpdateTransferOrderRequest request)
    {
        await _transferOrderHttpClient.UpdateTransferOrderAsync(transferOrderId, request);
    }

    /// <summary>
    /// Deletes the transfer order that the user has created before and has not yet been processed.
    /// </summary>
    /// <param name="transferOrderId"></param>
    [Authorize(Policy = "EmoneyTransferOrder:Delete")]
    [HttpDelete("{transferOrderId}")]
    public async Task DeleteTransferOrderAsync(Guid transferOrderId)
    {
        await _transferOrderHttpClient.DeleteTransferOrderAsync(transferOrderId);
    }
}
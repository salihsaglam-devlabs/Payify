using LinkPara.Emoney.Application.Features.TransferOrders;
using LinkPara.Emoney.Application.Features.TransferOrders.Commands.CreateTransferOrder;
using LinkPara.Emoney.Application.Features.TransferOrders.Commands.DeleteTransferOrder;
using LinkPara.Emoney.Application.Features.TransferOrders.Commands.UpdateTransferOrder;
using LinkPara.Emoney.Application.Features.TransferOrders.Queries.GetTransferOrderById;
using LinkPara.Emoney.Application.Features.TransferOrders.Queries.GetTransferOrdersByUserId;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Emoney.API.Controllers;

public class TransferOrdersController : ApiControllerBase
{
    /// <summary>
    /// Returns all transfer orders summary of the user as a list.
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyTransferOrder:ReadAll")]
    [HttpGet("")]
    public async Task<ActionResult<PaginatedList<TransferOrderDto>>> GetTransferOrdersByUserIdAsync([FromQuery] GetTransferOrdersByUserIdQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Returns a transfer orders detail of the user.
    /// </summary>
    /// <param name="transferOrderId">Transfer Order id</param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyTransferOrder:Read")]
    [HttpGet("{transferOrderId}")]
    public async Task<ActionResult<TransferOrderDto>> GetTransferOrderByIdAsync(Guid transferOrderId)
    {
        return await Mediator.Send(new GetTransferOrderByIdQuery { Id = transferOrderId });
    }

    /// <summary>
    /// Creates a new transfer order for the user
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyTransferOrder:Create")]
    [HttpPost("")]
    public async Task CreateTransferOrderAsync(CreateTransferOrderCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Updates the transfer order that the user has created before and has not yet been processed.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyTransferOrder:Update")]
    [HttpPut("{transferOrderId}")]
    public async Task UpdateTransferOrderAsync(UpdateTransferOrderCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Deletes the transfer order that the user has created before and has not yet been processed.
    /// </summary>
    /// <param name="transferOrderId">Transfer Order id</param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyTransferOrder:Delete")]
    [HttpDelete("{transferOrderId}")]
    public async Task DeleteTransferOrderAsync(Guid transferOrderId)
    {
        await Mediator.Send(new DeleteTransferOrderCommand { Id = transferOrderId });
    }
}
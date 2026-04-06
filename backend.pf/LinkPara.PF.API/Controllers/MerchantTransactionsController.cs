using LinkPara.PF.Application.Commons.Models.MerchantTransactions;
using LinkPara.PF.Application.Features.MerchantTransactions;
using LinkPara.PF.Application.Features.MerchantTransactions.Command;
using LinkPara.PF.Application.Features.MerchantTransactions.Command.GenerateOrderNumber;
using LinkPara.PF.Application.Features.MerchantTransactions.Command.PatchMerchantTransaction;
using LinkPara.PF.Application.Features.MerchantTransactions.Queries.GetAllMerchantInstallmentTransactions;
using LinkPara.PF.Application.Features.MerchantTransactions.Queries.GetAllMerchantTransactions;
using LinkPara.PF.Application.Features.MerchantTransactions.Queries.GetMerchantTransactionById;
using LinkPara.PF.Application.Features.MerchantTransactions.Queries.GetMerchantTransactionStatus;
using LinkPara.PF.Application.Features.Payments.Commands.ManualReturn;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.PF.API.Controllers;

public class MerchantTransactionsController : ApiControllerBase
{
    /// <summary>
    /// Returns all transactions
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantTransaction:ReadAll")]
    [HttpGet("")]
    public async Task<ActionResult<PaginatedList<MerchantTransactionDto>>> GetAllAsync([FromQuery] GetAllMerchantTransactionQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Return a transaction
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantTransaction:Read")]
    [HttpGet("{id}")]
    public async Task<ActionResult<MerchantTransactionDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetMerchantTransactionByIdQuery { Id = id });
    }

    [Authorize(Policy = "MerchantTransaction:Update")]
    [HttpPatch("{id}")]
    public async Task<UpdateMerchantTransactionRequest> Patch(Guid id, [FromBody] JsonPatchDocument<UpdateMerchantTransactionRequest> merchantTransaction)
    {
        return await Mediator.Send(new PatchMerchantTransactionCommand { Id = id, MerchantTransaction = merchantTransaction });
    }

    /// <summary>
    /// Return a transaction status counts
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantTransaction:ReadAll")]
    [HttpGet("statusCount")]
    public async Task<List<MerchantTransactionStatusModel>> GetStatusCountAsync([FromQuery] GetMerchantTransactionStatusQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Generate order number
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantTransaction:Read")]
    [HttpPost("generate-orderNumber")]
    public async Task<OrderNumberResponse> GenerateUniqueOrderNumberAsync(GenerateOrderNumberCommand command)
    {
        return await Mediator.Send(command);
    }
    
    /// <summary>
    /// Manual Return order
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "MerchantTransaction:Read")]
    [HttpPost("manual-return")]
    public async Task ReturnAsync(ManualReturnCommand request)
    {
        await Mediator.Send(request);
    }

    /// <summary>
    /// Returns all installment transactions
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantTransaction:ReadAll")]
    [HttpGet("merchantInstallmentTransactions")]
    public async Task<ActionResult<PaginatedList<MerchantInstallmentTransactionDto>>> GetAllAsync([FromQuery] GetAllMerchantInstallmentTransactionQuery query)
    {
        return await Mediator.Send(query);
    }
}

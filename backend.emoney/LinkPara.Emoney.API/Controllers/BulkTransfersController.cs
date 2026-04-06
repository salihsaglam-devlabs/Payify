using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LinkPara.Emoney.Application.Features.BulkTransfers.Commands.SaveBulkTransfer;
using MediatR;
using LinkPara.Emoney.Application.Features.BulkTransfers.Queries.GetAllBulkTransfer;
using LinkPara.SharedModels.Pagination;
using LinkPara.Emoney.Application.Features.BulkTransfers;
using LinkPara.Emoney.Application.Features.BulkTransfers.Queries.GetBulkTransferById;
using LinkPara.Emoney.Application.Features.BulkTransfers.Commands.ApproveBulkTransfer;
using LinkPara.Emoney.Application.Features.BulkTransfers.Queries.GetAllReportBulkTransfer;

namespace LinkPara.Emoney.API.Controllers;

public class BulkTransfersController : ApiControllerBase
{
    /// <summary>
    /// save bulk transfer
    /// </summary>
    /// <returns></returns>
    [HttpPost("")]
    [Authorize(Policy = "BulkTransfer:Create")]
    public async Task<Unit> SaveAsync([FromBody] SaveBulkTransferCommand command)
    {
        return await Mediator.Send(command);
    }

    /// <summary>
    /// get bulk transfers
    /// </summary>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "BulkTransfer:ReadAll")]
    public async Task<PaginatedList<BulkTransferDto>> GetListAsync([FromQuery] GetAllBulkTransferQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// get bulk transfer
    /// </summary>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "BulkTransfer:Read")]
    public async Task<BulkTransferDto> GetByIdAsync([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetBulkTransferByIdQuery { Id = id, CalculateCommission = true });
    }

    /// <summary>
    /// action bulk transfer
    /// </summary>
    /// <returns></returns>
    [HttpPut("action")]
    [Authorize(Policy = "BulkTransfer:Update")]
    public async Task<Unit> ActionBulkTransferAsync([FromBody]ActionBulkTransferCommand command)
    {
        return await Mediator.Send(command);
    }

    /// <summary>
    /// get report bulk transfers
    /// </summary>
    /// <returns></returns>
    [HttpGet("report")]
    [Authorize(Policy = "BulkTransferReport:ReadAll")]
    public async Task<PaginatedList<BulkTransferDto>> GetReportListAsync([FromQuery] GetAllReportBulkTransferQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// get report bulk transfer
    /// </summary>
    /// <returns></returns>
    [HttpGet("report/{id}")]
    [Authorize(Policy = "BulkTransferReport:Read")]
    public async Task<BulkTransferDto> GetReportByIdAsync([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetBulkTransferByIdQuery { Id = id, CalculateCommission = false });
    }

}

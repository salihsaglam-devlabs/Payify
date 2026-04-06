using LinkPara.Emoney.Application.Features.ManualTransfer;
using LinkPara.Emoney.Application.Features.ManualTransfer.Commands;
using LinkPara.Emoney.Application.Features.ManualTransfer.Queries.GetAllManualTransfers;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Emoney.API.Controllers;

public class ManualTransfersController : ApiControllerBase
{
    
    [Authorize(Policy = "EmoneyWallet:ReadAll")]
    [HttpGet("")]
    public async Task<PaginatedList<ManualTransferDto>> GetAllAsync([FromQuery] GetAllManualTransfersQuery query)
    {
        return await Mediator.Send(query);
    }
    
    [Authorize(Policy = "EmoneyWallet:Create")]
    [HttpPost("")]
    public async Task CreateManualTransferAsync([FromBody] CreateManualTransferCommand command)
    {
        var approvalId = Request.Headers["ApprovalRequestId"];
        if (!string.IsNullOrEmpty(approvalId))
        {
            command.ApprovalId = Guid.Parse(approvalId);
        }
        await Mediator.Send(command);
    }
}
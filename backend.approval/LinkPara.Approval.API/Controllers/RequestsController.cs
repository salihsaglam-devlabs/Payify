using LinkPara.Approval.Application.Features.Requests;
using LinkPara.Approval.Application.Features.Requests.Commands.ApproveRequest;
using LinkPara.Approval.Application.Features.Requests.Commands.PatchRequest;
using LinkPara.Approval.Application.Features.Requests.Commands.RejectRequest;
using LinkPara.Approval.Application.Features.Requests.Commands.SaveRequest;
using LinkPara.Approval.Application.Features.Requests.Queries.GetRequestById;
using LinkPara.Approval.Application.Features.Requests.Queries.GetRequests;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Approval.API.Controllers;

public class RequestsController : ApiControllerBase
{
    [Authorize(Policy = "ApprovalRequest:Create")]
    [HttpPost("")]
    public async Task<ApprovalResponse> SaveAsync(SaveRequestCommand command)
    {
        return await Mediator.Send(command);
    }
    [Authorize(Policy = "ApprovalRequest:Update")]
    [HttpPost("approve")]
    public async Task<ApprovalResponse> ApproveAsync(ApproveRequestCommand command)
    {
        return await Mediator.Send(command);
    }

    [Authorize(Policy = "ApprovalRequest:Update")]
    [HttpPost("reject")]
    public async Task RejectAsync(RejectRequestCommand command)
    {
        await Mediator.Send(command);
    }

    [Authorize(Policy = "ApprovalRequest:Read")]
    [HttpGet("{requestId}")]
    public async Task<RequestDto> GetAsync(Guid requestId)
    {
        return await Mediator.Send(new GetRequestByIdQuery { Id = requestId });
    }

    [Authorize(Policy = "ApprovalRequest:ReadAll")]
    [HttpGet("")]
    public async Task<PaginatedList<RequestDto>> GetAllRequestsAsync([FromQuery] GetRequestsQuery query)
    {
        return await Mediator.Send(query);
    }

    [Authorize(Policy = "ApprovalRequest:Update")]
    [HttpPatch("{requestId}")]
    public async Task PatchAsync(Guid requestId, [FromBody] JsonPatchDocument<PatchRequestDto> patchRequestDto)
    {
        await Mediator.Send(new PatchRequestCommand { RequestId = requestId, PatchRequestDto = patchRequestDto });
    }

    [Authorize(Policy = "ApprovalRequest:Create")]
    [HttpPost("duplicate")]
    public async Task<ApprovalResponse> DuplicateAsync(DuplicateRequestCommand command)
    {
        return await Mediator.Send(command);
    }

    [Authorize(Policy = "CashbackRule:ReadAll")]
    [HttpGet("cashback")]
    public async Task<PaginatedList<RequestCashbackDto>> GetAllCashbackRequestsAsync([FromQuery] GetCashbackRequestsQuery query)
    {
        return await Mediator.Send(query);
    }

    [Authorize(Policy = "WalletBlockage:ReadAll")]
    [HttpGet("wallet-blockage")]
    public async Task<PaginatedList<RequestWalletBlockageDto>> GetAllWalletBlockageRequestsAsync([FromQuery] GetWalletBlockageRequestsQuery query)
    {
        return await Mediator.Send(query);
    }
}

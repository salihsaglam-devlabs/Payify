using LinkPara.ApiGateway.BackOffice.Services.Approval.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Approval.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Approval;

public class RequestController : ApiControllerBase
{
    private readonly IApprovalHttpClient _approvalHttpClient;

    public RequestController(IApprovalHttpClient approvalHttpClient)
    {
        _approvalHttpClient = approvalHttpClient;
    }
    
    [HttpGet("{id}")]
    [Authorize(Policy = "Approval:Read")]
    public async Task<ApprovalRequestSummaryDto> GetRequestById([FromRoute] Guid id)
    {
        return await _approvalHttpClient.GetRequestByIdAsync(id);
    }

    [HttpPost("duplicate")]
    [Authorize(Policy = "ApprovalRequest:Create")]
    public async Task<ApprovalResponse> DuplicateAsync(DuplicateRequest request)
    {
        return await _approvalHttpClient.DuplicateRequestAsync(request);
    }
}
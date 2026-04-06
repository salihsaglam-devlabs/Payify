using LinkPara.ApiGateway.BackOffice.Services.Approval.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Approval.Models;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Approval;

public class ApprovalController : ApiControllerBase
{
    private readonly IApprovalHttpClient _approvalHttpClient;

    public ApprovalController(IApprovalHttpClient approvalHttpClient)
    {
        _approvalHttpClient = approvalHttpClient;
    }

    /// <summary>
    /// Approve a request.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("approve")]
    [Authorize(Policy = "Approval:Create")]
    public async Task<ApprovalResponse> ApproveAsync(BaseApproveRequest request)
    {
        return await _approvalHttpClient.ApproveAsync(request);
    }

    /// <summary>
    /// Reject a request.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("reject")]
    [Authorize(Policy = "Approval:Create")]
    public async Task RejectAsync(BaseRejectRequest request)
    {
        await _approvalHttpClient.RejectAsync(request);
    }

    /// <summary>
    /// Get all authorized requests.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "Approval:ReadAll")]
    public async Task<PaginatedList<RequestDto>> GetAllAuhorizedRequests([FromQuery] GetFilterApprovalRequest request)
    {
        return await _approvalHttpClient.GetAllAuthorizedRequests(request);
    }

    /// <summary>
    /// Get request with screen fields.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "Approval:Read")]
    public async Task<RequestScreenFields> GetRequestWithScreenFieldsAsync([FromRoute] Guid id)
    {
        return await _approvalHttpClient.GetRequestWithScreenFieldsAsync(id);
    }
}

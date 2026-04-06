using LinkPara.Approval;
using LinkPara.Approval.Models;
using LinkPara.Approval.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Identity.API.Controllers;

public class ApprovalsController : ApiControllerBase
{
    private readonly IApprovalService _approvalScreenService;

    public ApprovalsController(IApprovalService approvalScreenService)
    {
        _approvalScreenService = approvalScreenService;
    }

    /// <summary>
    /// Gets approval screen information 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Approval:Read")]
    [HttpGet("")]
    public async Task<ApprovalScreenResponse> GetApprovalScreenFields([FromQuery]ApprovalScreenRequest request)
    {
        return await _approvalScreenService.GetScreenFieldsAsync(request);
    }
}

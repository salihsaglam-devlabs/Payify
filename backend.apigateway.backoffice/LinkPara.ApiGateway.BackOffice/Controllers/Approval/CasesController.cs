using LinkPara.ApiGateway.BackOffice.Filters.MakerChecker;
using LinkPara.ApiGateway.BackOffice.Services.Approval.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Approval.Models;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Approval;

public class CasesController : ApiControllerBase
{
    private readonly IApprovalHttpClient _approvalHttpClient;
    private readonly ICaseContainer _caseContainer;
    public CasesController(IApprovalHttpClient approvalHttpClient, ICaseContainer caseContainer)
    {
        _approvalHttpClient = approvalHttpClient;
        _caseContainer = caseContainer;
    }

    /// <summary>
    /// GetAllCases.
    /// </summary>
    /// <returns></returns>
    [HttpGet("get-all-cases")]
    [Authorize(Policy = "Case:ReadAll")]
    public async Task<PaginatedList<CaseDto>> GetAllCasesAsync([FromQuery] GetFilterCaseRequest request)
    {
        return await _approvalHttpClient.GetAllCasesAsync(request);
    }

    /// <summary>
    /// GetCaseById.
    /// </summary>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "Case:Read")]
    public async Task<CaseDto> GetCaseByIdAsync(Guid id)
    {
        return await _approvalHttpClient.GetCaseByIdAsync(id);
    }

    /// <summary>
    /// update a case.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("")]
    [Authorize(Policy = "Case:Update")]
    public async Task UpdateCaseAsync(UpdateCaseRequest request)
    {
        await _approvalHttpClient.UpdateCaseAsync(request);

        await _caseContainer.LoadAllCasesCacheAsync();
    }

    /// <summary>
    /// save a makerchecker.
    /// </summary>
    /// <param name="caseId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("{caseId}/makerchecker")]
    [Authorize(Policy = "MakerChecker:Create")]
    public async Task SaveMakerCheckerAsync([FromRoute] Guid caseId, SaveMakerCheckerRequest request)
    {
        request.CaseId = caseId;
        await _approvalHttpClient.SaveMakerCheckerAsync(request);
    }

    /// <summary>
    /// update a makerchecker.
    /// </summary>
    /// <param name="caseId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("{caseId}/makerchecker")]
    [Authorize(Policy = "MakerChecker:Update")]
    public async Task UpdateMakerCheckerAsync([FromRoute] Guid caseId, UpdateMakerCheckerRequest request)
    {
        request.CaseId = caseId;
        await _approvalHttpClient.UpdateMakerCheckerAsync(request);
    }

    /// <summary>
    /// update a makerchecker.
    /// </summary>
    /// <param name="caseId"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{caseId}/makerchecker/{id}")]
    [Authorize(Policy = "MakerChecker:Delete")]
    public async Task DeleteMakerCheckerAsync([FromRoute] Guid caseId, Guid id)
    {
        await _approvalHttpClient.DeleteMakerCheckerAsync(id);
    }
}

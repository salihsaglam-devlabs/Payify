using LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients.PhysicalPos;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests.PhysicalPos;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses.PhysicalPos;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Pf;

public class PhysicalPosReconciliationController : ApiControllerBase
{
    private readonly IPhysicalPosReconciliationHttpClient _physicalPosReconciliationHttpClient;
    
    public PhysicalPosReconciliationController(IPhysicalPosReconciliationHttpClient physicalPosReconciliationHttpClient)
    {
        _physicalPosReconciliationHttpClient = physicalPosReconciliationHttpClient;
    }
    
    /// <summary>
    /// Get all EndOfDay records
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "PhysicalPosReconciliation:ReadAll")]
    [HttpGet("")]
    public async Task<ActionResult<PaginatedList<PhysicalPosEndOfDayDto>>> GetFilterAsync([FromQuery] GetAllPhysicalPosEndOfDayRequest request)
    {
        return await _physicalPosReconciliationHttpClient.GetAllPhysicalPosEndOfDayAsync(request);
    }
    
    /// <summary>
    /// Get EndOfDay with all related transactions
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "PhysicalPosReconciliation:Read")]
    [HttpGet("{id}")]
    public async Task<ActionResult<PhysicalPosEndOfDayDetailResponse>> GetByIdAsync([FromRoute] Guid id)
    {
        return await _physicalPosReconciliationHttpClient.GetDetailsByIdAsync(id);
    }
    
    /// <summary>
    /// Download EndOfDay excel with all related transactions
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}/download")]
    [Authorize(Policy = "PhysicalPosReconciliation:Read")]
    public async Task<IActionResult> DownloadEndOfDayExcelAsync([FromRoute] Guid id)
    {
        var response = await _physicalPosReconciliationHttpClient.DownloadReconciliationReportWithBytesAsync(id);

        var stream = await response.Content.ReadAsStreamAsync();
        var contentDisposition = response.Content.Headers.ContentDisposition;
        var fileName = contentDisposition?.FileName?.Trim('"') ?? "statement";
        var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";

        return File(stream, contentType, fileName);
    }
    
    /// <summary>
    /// Batch Manual Reconciliation
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "PhysicalPosReconciliation:Create")]
    [HttpPost("batch-manual-reconciliation")]
    public async Task BatchManualReconciliationAsync(BatchManualReconciliationRequest request)
    {
        await _physicalPosReconciliationHttpClient.BatchManualReconciliationAsync(request);
    }
    
    /// <summary>
    /// Singular Manual Reconciliation
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "PhysicalPosReconciliation:Create")]
    [HttpPost("single-manual-reconciliation")]
    public async Task SingleManualReconciliationAsync(SingleManualReconciliationRequest request)
    {
        await _physicalPosReconciliationHttpClient.SingleManualReconciliationAsync(request);
    }
}
using LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Pf;

public class MerchantStatementController : ApiControllerBase
{
    private readonly IMerchantStatementHttpClient _merchantStatementHttpClient;

    public MerchantStatementController(IMerchantStatementHttpClient merchantStatementHttpClient)
    {
        _merchantStatementHttpClient = merchantStatementHttpClient;
    }

    /// <summary>
    /// Get Filtered MerchantStatements
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "MerchantStatement:ReadAll")]
    public async Task<PaginatedList<MerchantStatementDto>> GetMerchantStatementsAsync([FromQuery] GetFilterMerchantStatementRequest request)
    {
        return await _merchantStatementHttpClient.GetMerchantStatementsAsync(request);
    }
    
    /// <summary>
    /// Download merchant statement
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("download")]
    [Authorize(Policy = "MerchantStatement:ReadAll")]
    public async Task<IActionResult> DownloadMerchantStatementAsync([FromQuery] DownloadMerchantStatementRequest request)
    {
        var response = await _merchantStatementHttpClient.GetMerchantStatementWithBytesAsync(request);

        var stream = await response.Content.ReadAsStreamAsync();
        var contentDisposition = response.Content.Headers.ContentDisposition;
        var fileName = contentDisposition?.FileName?.Trim('"') ?? "statement";
        var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";

        return File(stream, contentType, fileName);
    }
}
using LinkPara.ApiGateway.BackOffice.Services.BTrans.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.BTrans.Models;
using LinkPara.ApiGateway.BackOffice.Services.BTrans.Models.Request;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace LinkPara.ApiGateway.BackOffice.Controllers.BTrans;

public class BTransDocumentsController : ApiControllerBase
{
    private readonly IBTransDocumentClient _bTransDocumentHttpClient;

    public BTransDocumentsController(IBTransDocumentClient bTransDocumentHttpClient)
    {
        _bTransDocumentHttpClient = bTransDocumentHttpClient;
    }
    
    [Authorize(Policy = "BTransDocument:ReadAll")]
    [HttpGet("")]
    public async Task<ActionResult<PaginatedList<BtransDocumentDto>>> GetAllAsync([FromQuery] GetDocumentsRequest request)
    {
        return await _bTransDocumentHttpClient.GetAllDocumentsAsync(request);
    }
    
    /// <summary>
    /// Get the parquet file content 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "BTransDocument:Read")]
    [HttpGet("{id}/get-content/")]
    public async Task<ParquetContentDto> GetParquetContentAsync([FromRoute] Guid id)
    {
        return await _bTransDocumentHttpClient.GetDocumentContentAsync(id);
    }
}
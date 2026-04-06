using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using LinkPara.ApiGateway.Merchant.Services.Document.HttpClients;
using LinkPara.ApiGateway.Merchant.Services.Document.Models;

namespace LinkPara.ApiGateway.Merchant.Controllers.Document;

public class DocumentTypesController : ApiControllerBase
{
    private readonly IDocumentTypeHttpClient _documentTypeHttpClient;

    public DocumentTypesController(IDocumentTypeHttpClient documentTypeHttpClient)
    {
        _documentTypeHttpClient = documentTypeHttpClient;
    }

    [Authorize(Policy = "DocumentType:Read")]
    [HttpGet("{Id}")]
    public async Task<DocumentTypeDto> GetDocumentTypeAsync(Guid Id)
    {
        return await _documentTypeHttpClient.GetDocumentTypeAsync(Id);
    }

    [Authorize(Policy = "DocumentType:ReadAll")]
    [HttpGet("")]
    public async Task<PaginatedList<DocumentTypeDto>> GetDocumentTypesAsync([FromQuery] GetDocumentTypesRequest request)
    {
        return await _documentTypeHttpClient.GetDocumentTypesAsync(request);
    }

    [Authorize(Policy = "DocumentType:Create")]
    [HttpPost("")]
    public async Task CreateDocumentTypeAsync(DocumentTypeDto request)
    {
        await _documentTypeHttpClient.CreateDocumentTypeAsync(request);
    }

    [Authorize(Policy = "DocumentType:Delete")]
    [HttpDelete("{Id}")]
    public async Task DeleteDocumentTypeAsync(Guid Id)
    {
        await _documentTypeHttpClient.DeleteDocumentTypeAsync(Id);
    }
}

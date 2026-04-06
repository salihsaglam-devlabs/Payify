using System.ComponentModel.DataAnnotations;
using LinkPara.ApiGateway.BackOffice.Services.Document.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Document.Models;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Document;

public class DocumentsController : ApiControllerBase
{
    private readonly IDocumentHttpClient _documentHttpClient;
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(IDocumentHttpClient documentHttpClient, ILogger<DocumentsController> logger)
    {
        _documentHttpClient = documentHttpClient;
        _logger = logger;
    }

    [Authorize(Policy = "Document:Read")]
    [HttpGet("{Id}")]
    public async Task<DocumentDto> GetDocumentAsync(Guid Id)
    {
        return await _documentHttpClient.GetDocumentAsync(Id);
    }

    [Authorize(Policy = "Document:ReadAll")]
    [HttpGet]
    public async Task<PaginatedList<DocumentResponse>> GetDocumentsAsync([FromQuery] GetDocumentsRequest request)
    {
        try
        {
            return await _documentHttpClient.GetDocumentsAsync(request);
        }
        catch (Exception exception)
        {
            _logger.LogError($"GetDocumentsError Gateway: {exception}");
            throw;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [Authorize(Policy = "Document:Create")]
    [HttpPost()]
    public async Task<DocumentResponse> SaveDocumentAsync([Required] IFormFile file, [Required] Guid documentTypeId, Guid? userId = null, Guid? merchantId = null, Guid? accountId = null, Guid? subMerchantId = null)
    {
        await using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);

        var request = new DocumentDto()
        {
            OriginalFileName = file.FileName,
            ContentType = file.ContentType,
            Bytes = memoryStream.ToArray(),
            UserId = userId,
            MerchantId = merchantId,
            SubMerchantId = subMerchantId,
            DocumentTypeId = documentTypeId,
            AccountId = accountId
        };
        return await _documentHttpClient.SaveDocumentAsync(request);
    }

    /// <summary>
    /// 
    /// </summary>
    [Authorize(Policy = "Document:Delete")]
    [HttpDelete("{id}")]
    public async Task DeleteDocumentAsync(Guid id)
    {
        await _documentHttpClient.DeleteDocumentAsync(id);
    }

    [Authorize(Policy = "Document:Update")]
    [HttpPut()]
    public async Task<DocumentResponse> UpdateDocumentAsync([Required] IFormFile file, [Required] Guid documentTypeId, Guid? Id = null, Guid? userId = null, Guid? merchantId = null, Guid? accountId = null, Guid? subMerchantId = null)
    {
        await using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);

        var request = new UpdateDocumentDto()
        {
            OriginalFileName = file.FileName,
            ContentType = file.ContentType,
            Bytes = memoryStream.ToArray(),
            UserId = userId,
            MerchantId = merchantId,
            SubMerchantId = subMerchantId,
            AccountId = accountId,
            DocumentTypeId = documentTypeId,
            Id = Id
        };
        return await _documentHttpClient.UpdateDocumentAsync(request);
    }
}
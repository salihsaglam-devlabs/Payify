using LinkPara.ApiGateway.CorporateWallet.Services.Document.HttpClients;
using LinkPara.ApiGateway.CorporateWallet.Services.Document.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.CorporateWallet.Controllers.Document;

public class DocumentsController : ApiControllerBase
{
    private readonly IDocumentHttpClient _documentHttpClient;

    public DocumentsController(IDocumentHttpClient documentHttpClient)
    {
        _documentHttpClient = documentHttpClient;
    }

    [Authorize(Policy = "Document:Read")]
    [HttpGet("{Id}")]
    public async Task<DocumentDto> GetDocumentAsync(Guid Id)
    {
        return await _documentHttpClient.GetDocumentAsync(Id);
    }

}

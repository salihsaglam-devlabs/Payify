using LinkPara.Documents.Application.Features.Documents;
using LinkPara.Documents.Application.Features.Documents.Commands;
using LinkPara.Documents.Application.Features.Documents.Queries;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Documents.API.Controllers
{
    public class DocumentsController : ApiControllerBase
    {
        [Authorize(Policy = "Document:Read")]
        [HttpGet("{Id}")]
        public async Task<DocumentDto> GetDocumentAsync([FromRoute] GetDocumentQuery query)
        {
            return await Mediator.Send(query);
        }

        [Authorize(Policy = "Document:ReadAll")]
        [HttpGet]
        public async Task<PaginatedList<DocumentResponse>> GetDocumentsAsync([FromQuery] GetDocumentListQuery query)
        {
            return await Mediator.Send(query);
        }

        [Authorize(Policy = "Document:Create")]
        [HttpPost]
        public async Task<DocumentResponse> SaveDocumentAsync(DocumentDto request)
        {
            return await Mediator.Send(new SaveDocumentCommand() { Document = request });
        }

        [Authorize(Policy = "Document:Delete")]
        [HttpDelete("{Id}")]
        public async Task DeleteDocumentAsync([FromRoute] DeleteDocumentCommand command)
        {
            await Mediator.Send(command);
        }
    }
}

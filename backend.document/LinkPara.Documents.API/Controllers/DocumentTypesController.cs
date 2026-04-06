using LinkPara.Documents.Application.Features.DocumentTypes;
using LinkPara.Documents.Application.Features.DocumentTypes.Commands;
using LinkPara.Documents.Application.Features.DocumentTypes.Queries;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Documents.API.Controllers
{
    public class DocumentTypesController : ApiControllerBase
    {
        [Authorize(Policy = "DocumentType:Read")]
        [HttpGet("{Id}")]
        public async Task<DocumentTypeDto> GetDocumentTypeAsync([FromRoute] GetDocumentTypeQuery query)
        {
            return await Mediator.Send(query);
        }

        [Authorize(Policy = "DocumentType:ReadAll")]
        [HttpGet]
        public async Task<PaginatedList<DocumentTypeDto>> GetDocumentTypesAsync([FromQuery] GetDocumentTypeListQuery query)
        {
            return await Mediator.Send(query);
        }

        [Authorize(Policy = "DocumentType:Create")]
        [HttpPost]
        public async Task CreateDocumentTypeAsync(CreateDocumentTypeCommand command)
        {
            await Mediator.Send(command);
        }

        [Authorize(Policy = "DocumentType:Delete")]
        [HttpDelete("{Id}")]
        public async Task DeleteDocumentTypeAsync([FromRoute] DeleteDocumentTypeCommand command)
        {
            await Mediator.Send(command);
        }
    }
}

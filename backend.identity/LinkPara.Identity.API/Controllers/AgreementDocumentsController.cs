using Microsoft.AspNetCore.Mvc;
using LinkPara.Identity.Application.Features.AgreementDocuments.Queries;
using LinkPara.Identity.Application.Features.AgreementDocuments.Queries.GetAllDocuments;
using LinkPara.Identity.Application.Features.AgreementDocuments.Commands.CreateDocument;
using LinkPara.Identity.Application.Features.AgreementDocuments;
using LinkPara.Identity.Application.Features.AgreementDocuments.Queries.GetDocumentById;
using LinkPara.Identity.Application.Features.AgreementDocuments.Queries.GetAllAgreementDocument;
using LinkPara.SharedModels.Pagination;
using LinkPara.Identity.Application.Features.AgreementDocuments.Commands.CreateDocumentToUser;
using LinkPara.Identity.Application.Features.AgreementDocuments.Queries.GetAgreedUsersOfDocument;
using LinkPara.Identity.Application.Features.AgreementDocuments.Commands.UpdateDocument;
using LinkPara.Identity.Application.Features.AgreementDocuments.Commands.DeleteDocument;
using Microsoft.AspNetCore.Authorization;

namespace LinkPara.Identity.API.Controllers;

public class AgreementDocumentsController : ApiControllerBase
{
    /// <summary>
    /// Get All Documents
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<List<AgreementDocumentVersionDto>>> GetAllDocumentsAsync()
    {
        return await Mediator.Send(new GetAllDocumentsQuery());
    }

    /// <summary>
    /// get all documents
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "AgreementDocument:ReadAll")]
    [HttpGet("getAllDocument")]
    public async Task<ActionResult<PaginatedList<AgreementDocumentResponse>>> GetAllAgreementDocumentAsync([FromQuery] GetAllAgreementDocumentQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Create a new document
    /// </summary>
    /// <param name="command"></param>
    [Authorize(Policy = "AgreementDocument:Create")]
    [HttpPost]
    public async Task CreateDocumentAsync(CreateDocumentCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// User agreed a specific document
    /// </summary>
    /// <param name="command"></param>
    [AllowAnonymous]
    [HttpPost("{userId}")]
    public async Task CreateDocumentToUserAsync(CreateDocumentToUserCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Get Document by Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "AgreementDocument:Read")]
    [HttpGet("{id}")]
    public async Task<ActionResult<AgreementDocumentResponse>> GetDocumentByIdAsync([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetDocumentByIdQuery { AgreementDocumentId = id });
    }

    /// <summary>
    /// Updates document with puth
    /// </summary>
    /// <param name="document"></param>
    /// <returns></returns>
    [Authorize(Policy = "AgreementDocument:Update")]
    [HttpPut()]
    public async Task Update(UpdateAgreementDocumentCommand document)
    {
        await Mediator.Send(document);
    }

    /// <summary>
    /// Get all users who already agreed specified documentId
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "AgreementDocument:ReadAll")]
    [HttpGet("agreed-user-list")]
    public async Task<PaginatedList<AgreementUserDto>> GetAgreedUsersOfDocument([FromQuery]GetAgreedUsersOfDocumentQuery query)
    {
        return await Mediator.Send(query);
    }

    [Authorize(Policy = "AgreementDocument:Delete")]
    [HttpDelete("{id}")]
    public async Task DeleteAsync(Guid id)
    {
        await Mediator.Send(new DeleteDocumentCommand() { Id = id });
    }

}

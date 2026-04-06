using LinkPara.PF.Application.Commons.Models.SubMerchants;
using LinkPara.PF.Application.Features.SubMerchantDocuments.Commands.SaveSubMerchantDocument;
using LinkPara.PF.Application.Features.SubMerchantDocuments.Queries.GetAllSubMerchantDocuments;
using LinkPara.PF.Application.Features.SubMerchants.Queries.GetSubMerchantById;
using LinkPara.PF.Application.Features.SubMerchants;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LinkPara.PF.Application.Features.SubMerchantDocuments.Queries.GetSubMerchantDocumentById;
using LinkPara.PF.Application.Features.SubMerchantDocuments.Commands.DeleteSubMerchantDocument;
using LinkPara.PF.Application.Features.SubMerchantDocuments.Commands.UpdateSubMerchantDocument;

namespace LinkPara.PF.API.Controllers;

public class SubMerchantDocumentsController : ApiControllerBase
{
    /// <summary>
    /// Returns all sub merchant documents.
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "SubMerchantDocument:ReadAll")]
    [HttpGet("")]
    public async Task<PaginatedList<SubMerchantDocumentDto>> GetAllAsync([FromQuery] GetAllSubMerchantDocumentsQuery request)
    {
        return await Mediator.Send(request);
    }

    /// <summary>
    /// Returns a sub merchant document.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "SubMerchantDocument:Read")]
    [HttpGet("{id}")]
    public async Task<ActionResult<SubMerchantDocumentDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetSubMerchantDocumentByIdQuery { DocumentId = id });
    }

    /// <summary>
    /// Deletes a sub merchant.
    /// </summary>
    /// <param name="documentId"></param>
    /// <returns></returns>
    [Authorize(Policy = "SubMerchantDocument:Delete")]
    [HttpDelete("{documentId}")]
    public async Task DeleteAsync([FromRoute] Guid documentId)
    {
        await Mediator.Send(new DeleteSubMerchantDocumentCommand { DocumentId = documentId });
    }
    
    /// <summary>
    /// Creates a sub merchant document.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "SubMerchantDocument:Create")]
    [HttpPost("")]
    public async Task SaveAsync([FromBody] SaveSubMerchantDocumentCommand request)
    {
        await Mediator.Send(request);
    }
    
    /// <summary>
    /// Updates a sub merchant document.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "SubMerchantDocument:Update")]
    [HttpPut("")]
    public async Task UpdateAsync([FromBody] SubMerchantDocumentDto request)
    {
        await Mediator.Send(new UpdateSubMerchantDocumentCommand{ SubMerchantDocument = request});
    }
}

using LinkPara.ApiGateway.BackOffice.Services.Identity.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Requests.AgreementDocuments;
using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Responses.AgreementDocuments;
using LinkPara.ApiGateway.BackOffice.Utils;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Identity;

public class AgreementDocumentsController : ApiControllerBase
{
    private readonly IAgreementDocumentHttpClient _agreementHttpClient;

    public AgreementDocumentsController(IAgreementDocumentHttpClient agreementHttpClient)
    {
        _agreementHttpClient = agreementHttpClient;
    }

    /// <summary>
    /// Creates a new agreement.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("")]
    [Authorize(Policy = "AgreementDocument:Create")]
    public async Task CreateDocumentAsync(CreateDocumentRequest request)
    {
        await _agreementHttpClient.CreateDocumentAsync(request);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "AgreementDocument:Read")]
    public async Task<ActionResult<AgreementDocumentResponse>> GetByIdAsync([FromRoute] Guid id)
    {
        return await _agreementHttpClient.GetAgreementDocumentByIdAsync(id);
    }

    [HttpGet("{id:guid}/word")]
    [Authorize(Policy = "AgreementDocument:Read")]
    public async Task<IActionResult> GetByIdAsWordDocumentAsync([FromRoute] Guid id, string languageCode)
    {
        var response = await _agreementHttpClient.GetAgreementDocumentByIdAsync(id);
        var document = response.Agreements.First(x => x.LanguageCode == languageCode);
        var word = Word.CreateWordDocument(document.Content);
        return File(word, "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
           $"{document.Title}.docx");
    }

    [HttpGet()]
    [Authorize(Policy = "AgreementDocument:ReadAll")]
    public async Task<ActionResult<PaginatedList<AgreementDocumentResponse>>> GetDocumentsAsync([FromQuery] GetAllAgreementDocumentRequest request)
    {
        return await _agreementHttpClient.GetAllAgreementDocumentAsync(request);
    }

    /// <summary>
    /// Updates document with put
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut()]
    [Authorize(Policy = "AgreementDocument:Update")]
    public async Task UpdateAsync(UpdateAgreementDocumentRequest request)
    {
        await _agreementHttpClient.UpdateAsync(request);
    }

    /// <summary>
    /// Get all users who already agreed specified documentId
    /// </summary>
    /// <returns></returns>
    [HttpGet("agreed-user-list")]
    public async Task<ActionResult<PaginatedList<AgreementUserDto>>> AgreementUserList([FromQuery] AgreedUserListRequest request)
    {
        return await _agreementHttpClient.GetAgreedUsersOfDocument(request);
    }

    /// <summary>
    /// Delete agreement document
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("")]
    [Authorize(Policy = "AgreementDocument:Delete")]
    public async Task DeleteAsync(Guid id)
    {
        await _agreementHttpClient.DeleteAgreementDocumentAsync(id);
    }
}
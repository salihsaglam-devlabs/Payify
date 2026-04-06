using LinkPara.ApiGateway.CorporateWallet.Services.Identity.HttpClients;
using LinkPara.ApiGateway.CorporateWallet.Services.Identity.Models.Requests.AgreementDocuments;
using LinkPara.ApiGateway.CorporateWallet.Services.Identity.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.CorporateWallet.Controllers.Identity;

public class AgreementDocumentsController : ApiControllerBase
{
    private readonly IAgreementDocumentHttpClient _agreementHttpClient;

    public AgreementDocumentsController(IAgreementDocumentHttpClient agreementHttpClient)
    {
        _agreementHttpClient = agreementHttpClient;
    }

    /// <summary>
    /// Returns all active agreements.
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("")]
    public async Task<ActionResult<List<AgreementDocumentVersionDto>>> GetDocumentsAsync()
    {
        return await _agreementHttpClient.GetDocumentsAsync();
    }

    /// <summary>
    /// Makes the user has signed the given agreement.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("{userId}")]
    public async Task CreateDocumentToUserAsync([FromRoute] Guid userId, CreateUserDocumentRequest request)
    {
        if(userId != Guid.Parse(UserId))
        {
            throw new UnauthorizedAccessException();
        }

        await _agreementHttpClient.CreateUserDocumentAsync(new CreateDocumentToUserRequest
        {
            UserId = userId,
            AgreementDocumentId = request.AgreementDocumentId
        });
    }
}
using LinkPara.ApiGateway.Merchant.Services.Identity.HttpClients;
using LinkPara.ApiGateway.Merchant.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Identity.Models.Responses;
using LinkPara.SharedModels.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Merchant.Controllers.Identity;

public class SecurityPicturesController : ApiControllerBase
{
    private readonly ISecurityPictureHttpClient _securityPictureHttpClient;

    public SecurityPicturesController(ISecurityPictureHttpClient securityPictureHttpClient)
    {
        _securityPictureHttpClient = securityPictureHttpClient;
    }

    /// <summary>
    /// Returns all active security pictures for user selection.
    /// </summary>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<List<SecurityPictureDto>>> GetAllAsync()
    {
        return await _securityPictureHttpClient.GetAllAsync();
    }

    /// <summary>
    /// Saves the security picture selected by the user.
    /// </summary>
    [Authorize]
    [HttpPost("{pictureId}/select")]
    public async Task<ActionResult> SelectPictureAsync([FromRoute] Guid pictureId)
    {
        if (!Guid.TryParse(UserId, out var userGuidId))
        {
            throw new NotFoundException(nameof(User), UserId!);
        }
        
        var request = new CreateUserSecurityPictureRequest
        {
            UserId = userGuidId,
            SecurityPictureId = pictureId
        };

        await _securityPictureHttpClient.SelectPictureAsync(pictureId, request);
        return NoContent();
    }
}

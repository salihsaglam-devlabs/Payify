using LinkPara.ApiGateway.Services.Identity.HttpClients;
using LinkPara.ApiGateway.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.Services.Identity.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Controllers.Identity;

public class UsersController : ApiControllerBase
{
    private readonly IUserHttpClient _userHttpClient;

    public UsersController(IUserHttpClient userHttpClient)
    {
        _userHttpClient = userHttpClient;
    }

    /// <summary>
    /// Returns the user which is logged in by user id.
    /// </summary>
    [Authorize(Policy = "User:Read")]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetAsync()
    {
        return await _userHttpClient.GetUserAsync(new GetUserRequest { });
    }

    /// <summary>
    /// Updates the user.
    /// </summary>
    /// <param name="request"></param>
    [Authorize(Policy = "User:Update")]
    [HttpPut("")]
    public async Task UpdateAsync(UpdateUserRequest request)
    {
        await _userHttpClient.UpdateUserAsync(request);
    }

    /// <summary>
    /// Updates the user with kyc status if its not upgraded before.
    /// </summary>
    /// <param name="request"></param>
    [Authorize(Policy = "User:Update")]
    [HttpPut("kyc")]
    public async Task UpdateKycAsync(UpdateKycRequest request)
    {
        await _userHttpClient.UpdateKycAsync(request);
    }

    /// <summary>
    /// Returns all agreements by the user has signed or needs to sign it.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [Authorize(Policy = "User:Read")]
    [HttpGet("{userId}/agreementDocuments")]
    public async Task<ActionResult<List<UserAgreementDocumentsStatusDto>>> GetUserDocumentsAsync(Guid userId, [FromQuery] UserDocumentFilterRequest request)
    {
        if (userId != Guid.Parse(UserId))
        {
            throw new UnauthorizedAccessException();
        }
        return await _userHttpClient.GetUserDocumentsAsync(userId, request);
    }

    /// <summary>
    /// Returns User's last login activity dates and n login activity record.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [Authorize(Policy = "User:Read")]
    [HttpGet("{userId}/loginActivity")]
    public async Task<ActionResult<GetUserLoginActivityResponse>> GetUserLoginActivity(Guid userId)
    {
        return await _userHttpClient.GetUserLoginActivity(userId);
    }
}
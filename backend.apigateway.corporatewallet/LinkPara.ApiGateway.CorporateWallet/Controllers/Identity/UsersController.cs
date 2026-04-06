using AutoMapper;
using LinkPara.ApiGateway.CorporateWallet.Services.Identity.HttpClients;
using LinkPara.ApiGateway.CorporateWallet.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.Identity.Models.Responses;
using LinkPara.SharedModels.Authorization.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LinkPara.ApiGateway.CorporateWallet.Controllers.Identity;

public class UsersController : ApiControllerBase
{
    private readonly IUserHttpClient _userHttpClient;
    private readonly IMapper _mapper;

    public UsersController(IUserHttpClient userHttpClient,
        IMapper mapper)
    {
        _userHttpClient = userHttpClient;
        _mapper = mapper;
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

    /// <summary>
    /// Returns the user which is logged in by user id.
    /// </summary>
    [Authorize(Policy = "User:Read")]
    [HttpGet("me")]
    public async Task<ActionResult<UserWithPermissionsDto>> GetAsync()
    {
        var user = await _userHttpClient.GetUserAsync(new GetUserRequest { });


        var result = _mapper.Map<UserDto, UserWithPermissionsDto>(user);

        result.ClaimValues = User.Claims.Distinct()
            .Where(q => q.Type == ClaimKey.UserScope || q.Type == ClaimKey.RoleScope)?.OrderBy(q => q.Value)
            .Select(q => q.Value).ToList();

        result.RoleNames = User.Claims.Where(q => q.Type == ClaimTypes.Role)?.Select(q => q.Value).ToList();
        result.RoleIds = User.Claims.Where(c => c.Type == "RoleId")?.Select(c => c.Value).ToList();

        return result;

    }

    /// <summary>
    /// Returns all agreements by the user has signed or needs to sign it.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="request"></param>
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
    /// Returns role of the user.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [HttpGet("{userId}/roles")]
    [Authorize(Policy = "Role:Read")]
    public async Task<List<RoleDto>> GetUserRolesAsync(Guid userId)
    {
        return await _userHttpClient.GetUserRolesAsync(userId);
    }


}
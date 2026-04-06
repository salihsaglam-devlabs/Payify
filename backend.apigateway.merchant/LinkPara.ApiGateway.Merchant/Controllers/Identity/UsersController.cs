using AutoMapper;
using LinkPara.ApiGateway.Merchant.Services.Identity.HttpClients;
using LinkPara.ApiGateway.Merchant.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Identity.Models.Responses;
using LinkPara.SharedModels.Authorization.Models;
using LinkPara.SharedModels.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LinkPara.ApiGateway.Merchant.Controllers.Identity;

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
    /// Returns authenticated current user informations with permissions.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserWithPermissionsDto>> GetAsync()
    {
        if (!Guid.TryParse(UserId, out var userGuidId))
        {
            throw new NotFoundException(nameof(User), UserId!);
        }

        var user = await _userHttpClient.GetUserByIdAsync(userGuidId);

        if (user is null)
        {
            throw new NotFoundException(nameof(User));
        }

        var result = _mapper.Map<UserDto, UserWithPermissionsDto>(user);

        result.ClaimValues = User.Claims.Distinct()
            .Where(q => q.Type == ClaimKey.UserScope || q.Type == ClaimKey.RoleScope)?.OrderBy(q => q.Value)
            .Select(q => q.Value).ToList();

        result.RoleNames = User.Claims.Where(q => q.Type == ClaimTypes.Role)?.Select(q => q.Value).ToList();

        return result;
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
    /// Returns all agreements by the user has signed or needs to sign it.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [Authorize(Policy = "User:Read")]
    [HttpGet("{userId}/agreementDocuments")]
    public async Task<ActionResult<List<UserAgreementDocumentsStatusDto>>> GetUserDocumentsAsync(Guid userId)
    {
        if(userId != Guid.Parse(UserId))
        {
            throw new UnauthorizedAccessException();
        }
        return await _userHttpClient.GetUserDocumentsAsync(userId);
    }

    /// <summary>
    /// Verify user's email address
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("VerifyUserEmail")]
    public async Task<bool> VerifyUserEmail(VerifyEmailRequest request)
    {
        return await _userHttpClient.VerifyEmailAsync(request);
    }
}
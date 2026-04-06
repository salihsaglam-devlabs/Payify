using AutoMapper;
using LinkPara.ApiGateway.BackOffice.Commons.Models.AuthorizationModels;
using LinkPara.ApiGateway.BackOffice.Commons.Models.ExcelExportModels;
using LinkPara.ApiGateway.BackOffice.Commons.Models.IdentityModels;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;
using LinkPara.ApiGateway.BackOffice.Services.Identity.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Enums;
using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Responses;
using LinkPara.ApiGateway.BackOffice.Utils;
using LinkPara.ApiGateway.Commons.Helpers;
using LinkPara.SharedModels.Authorization.Models;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Identity;

public class UsersController : ApiControllerBase
{
    private readonly IUserHttpClient _userHttpClient;
    private readonly IWalletHttpClient _walletHttpClient;
    private readonly IMapper _mapper;
    private readonly IUserNameGenerator _userNameGenerator;

    public UsersController(IUserHttpClient userHttpClient,
        IWalletHttpClient walletHttpClient,
        IMapper mapper,
        IUserNameGenerator userNameGenerator)
    {
        _userHttpClient = userHttpClient;
        _walletHttpClient = walletHttpClient;
        _mapper = mapper;
        _userNameGenerator = userNameGenerator;
    }

    /// <summary>
    /// Returns user list.
    /// </summary>
    [HttpGet("")]
    [Authorize(Policy = "User:ReadAll")]
    public async Task<PaginatedList<UserDtoWithRoles>> GetAllUsersAsync([FromQuery] GetUsersRequest request)
    {
        return await _userHttpClient.GetAllUsersAsync(request);
    }

    /// <summary>
    /// Returns detailed informations of the user.
    /// </summary>
    /// <param name="userId"></param>
    [HttpGet("{userId}")]
    [Authorize(Policy = "User:Read")]
    public async Task<ActionResult<UserDetailDto>> GetDetailedUserAsync([FromRoute] Guid userId)
    {
        var userInfo = await _userHttpClient.GetUserDetailsAsync(userId);
        return userInfo;
    }

    /// <summary>
    /// Returns authenticated current user informations with permissions.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserWithPermissionsDto>> GetAsync()
    {
        if (!Guid.TryParse(UserId, out Guid userGuidId))
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
        result.RoleIds = User.Claims.Where(c => c.Type == "RoleId")?.Select(c => c.Value).ToList();

        return result;
    }

    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("")]
    [Authorize(Policy = "User:Create")]
    public async Task<UserCreateResponse> CreateAsync(CreateUserRequest request)
    {
        var user = _mapper.Map<CreateUserWithUserName>(request);
        user.UserName = await _userNameGenerator.GetUserName(UserTypePrefix.Internal, request.PhoneCode, request.PhoneNumber);
        user.UserType = UserType.Internal;

        return await _userHttpClient.CreateUserAsync(user);
    }

    /// <summary>
    /// Partial updates the user.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPatch("{userId}")]
    [Authorize(Policy = "User:Update")]
    public async Task PatchAsync(Guid userId, [FromBody] JsonPatchDocument<PatchUserRequest> request)
    {
        var user = await _userHttpClient.GetUserByIdAsync(userId);
        if (user is null)
        {
            throw new NotFoundException(nameof(user));
        }

        await _userHttpClient.PatchUserAsync(userId, request);
    }

    /// <summary>
    /// Appends role to the user.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("{userId}/roles")]
    [Authorize(Policy = "User:Update")]
    public async Task AddRoleUserAsync([FromRoute] Guid userId, UserRoleDto request)
    {
        await _userHttpClient.AssignUserRoleAsync(userId, request);
    }

    /// <summary>
    /// Returns role of the user.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [HttpGet("{userId}/roles")]
    [Authorize(Policy = "Role:Read")]
    public async Task<ActionResult<List<RoleDto>>> GetUserRolesAsync(Guid userId)
    {
        return await _userHttpClient.GetUserRolesAsync(userId);
    }

    /// <summary>
    /// Lists assigned claims to the user.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [HttpGet("{userId}/claims")]
    [Authorize(Policy = "Role:Read")]
    public async Task<ActionResult<List<ClaimDto>>> GetUserClaimsAsync(Guid userId)
    {
        return await _userHttpClient.GetUserClaimsAsync(userId);
    }

    /// <summary>
    /// Converts user wallets to individual.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [HttpPost("{userId}/convert-wallets-to-individual")]
    [Authorize(Policy = "User:Update")]
    public async Task ConvertUserWalletsToIndividualAsync([FromRoute] Guid userId)
    {
        await _walletHttpClient.ConvertUserWalletsToIndividualAsync(new ConvertUserWalletsToIndividualRequest()
        { UserId = userId });
    }

    /// <summary>
    /// Export users as Excel
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("excel")]
    [Authorize(Policy = "User:ReadAll")]
    public async Task<IActionResult> GetUsersExcelExportAsync(
        [FromQuery] GetUsersRequest request)
    {
        var response = await _userHttpClient.GetAllUsersAsync(request);
        var excelExportModel = _mapper.Map<List<UsersExcelExportModel>>(response.Items);
        var excel = Excel.Instance.CreateExcelDocument(excelExportModel);
        return File(excel, "application/vnd.ms-excel", $"users-{DateTime.Now.ToShortDateString()}.xlsx");
    }

    /// <summary>
    /// Returns User's last login activity dates and n login activity record.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [Authorize(Policy = "User:Read")]
    [HttpGet("{userId}/loginActivity")]
    public async Task<ActionResult<GetUserLoginActivityResponse>> GetUserLoginActivityAsync(Guid userId)
    {
        return await _userHttpClient.GetUserLoginActivityAsync(userId);
    }

    /// <summary>
    /// Removes User's lock out.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [Authorize(Policy = "User:Update")]
    [HttpPut("{userId}/removeUserLock")]
    public async Task RemoveUserLockAsync(Guid userId)
    {
        await _userHttpClient.RemoveUserLockAsync(userId);
    }

    /// <summary>
    /// Resends email verification mail
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "User:Read")]
    [HttpPost("ResendEmailVerify")]
    public async Task<bool> ResendEmailVerify(ResendEmailVerifyRequest request)
    {
        return await _userHttpClient.ResendEmailVerifyAsync(request);
    }
}
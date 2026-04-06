using LinkPara.ApiGateway.BackOffice.Commons.Models.IdentityModels;
using LinkPara.ApiGateway.BackOffice.Services.Identity.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Enums;
using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.ApiGateway.Commons.Helpers;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Pf;

public class MerchantUsersController : ApiControllerBase
{
    private readonly IMerchantUserService _merchantUserService;
    private readonly IUserHttpClient _userHttpClient;

    public MerchantUsersController(IMerchantUserService merchantUserService, IUserHttpClient userHttpClient)
    {
        _merchantUserService = merchantUserService;
        _userHttpClient = userHttpClient;
    }

    /// <summary>
    /// Returns a merchant users
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "MerchantUser:ReadAll")]
    public async Task<ActionResult<PaginatedList<MerchantUserDto>>> GetAllAsync([FromQuery] GetAllMerchantUserRequest request)
    {
        return await _merchantUserService.GetAllAsync(request);
    }

    /// <summary>
    /// Returns a merchant user
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "MerchantUser:Read")]
    public async Task<ActionResult<MerchantUserDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await _merchantUserService.GetByIdAsync(id);
    }

    /// <summary>
    /// Create a merchant user
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    /// <exception cref="AlreadyInUseException"></exception>
    [HttpPost("")]
    [Authorize(Policy = "MerchantUser:Create")]
    public async Task SaveAsync(SaveMerchantUserRequest request)
    {
        //var userPhone = await _userHttpClient.GetAllUsersAsync(new GetUsersRequest
        //{
        //    PhoneNumber = request.MobilePhoneNumber,
        //    UserStatus = UserStatus.Active,
        //    UserType = UserType.Corporate
        //});

        //foreach (var item in userPhone.Items)
        //{
        //    foreach (var role in item.Roles)
        //    {
        //        if (role.RoleScope == RoleScope.Merchant)
        //        {
        //            throw new AlreadyInUseException(_localizer.GetString("AlreadyInUseException"));
        //        }
        //    }
        //}

        //var userEmail = await _userHttpClient.GetAllUsersAsync(new GetUsersRequest
        //{
        //    Email = request.Email,
        //    UserStatus = UserStatus.Active,
        //    UserType = UserType.Corporate
        //});

        //foreach (var item in userEmail.Items)
        //{
        //    foreach (var role in item.Roles)
        //    {
        //        if (role.RoleScope == RoleScope.Merchant)
        //        {
        //            throw new AlreadyInUseException(_localizer.GetString("AlreadyInUseException"));
        //        }
        //    }
        //}

        await _merchantUserService.SaveAsync(request);
    }

    /// <summary>
    /// Update a merchant user
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("")]
    [Authorize(Policy = "MerchantUser:Update")]
    public async Task UpdateAsync(MerchantUserDto request)
    {
        await _merchantUserService.UpdateAsync(request);
    }

    /// <summary>
    /// Resets the security picture of a merchant user (admin only).
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [HttpDelete("{userId}/security-picture")]
    [Authorize(Policy = "MerchantUser:Update")]
    public async Task<ActionResult> ResetSecurityPictureAsync([FromRoute] Guid userId)
    {
        await _userHttpClient.ResetUserSecurityPictureAsync(userId);
        return NoContent();
    }
}

using LinkPara.ApiGateway.Merchant.Commons.Helpers;
using LinkPara.ApiGateway.Merchant.Commons.Models.IdentityModels;
using LinkPara.ApiGateway.Merchant.Services.Identity.HttpClients;
using LinkPara.ApiGateway.Merchant.Services.Identity.Models.Enums;
using LinkPara.ApiGateway.Merchant.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace LinkPara.ApiGateway.Merchant.Controllers.Pf;

public class MerchantUsersController : ApiControllerBase
{
    private readonly IMerchantUserHttpClient _merchantUserHttpClient;
    private readonly IUserHttpClient _userHttpClient;
    private readonly IUserNameGenerator _userNameGenerator;
    private readonly IStringLocalizer _localizer;

    public MerchantUsersController(IMerchantUserHttpClient merchantUserHttpClient,
        IUserHttpClient userHttpClient,
        IUserNameGenerator userNameGenerator,
        IStringLocalizerFactory factory)
    {
        _merchantUserHttpClient = merchantUserHttpClient;
        _userHttpClient = userHttpClient;
        _userNameGenerator = userNameGenerator;
        _localizer = factory.Create("Exceptions", "LinkPara.ApiGateway.Merchant");
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
        return await _merchantUserHttpClient.GetAllAsync(request);
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
        return await _merchantUserHttpClient.GetByIdAsync(id);
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
        var userPhone = await _userHttpClient.GetAllUsersAsync(new GetUsersRequest
        {
            PhoneNumber = request.MobilePhoneNumber,
            UserStatus = UserStatus.Active,
            UserType = UserType.Corporate
        });

        foreach (var item in userPhone.Items)
        {
            foreach (var role in item.Roles)
            {
                if (role.RoleScope == RoleScope.Merchant)
                {
                    throw new AlreadyInUseException(_localizer.GetString("AlreadyInUseException"));
                }
            }
        }

        var userEmail = await _userHttpClient.GetAllUsersAsync(new GetUsersRequest
        {
            Email = request.Email,
            UserStatus = UserStatus.Active,
            UserType = UserType.Corporate
        });

        foreach (var item in userEmail.Items)
        {
            foreach (var role in item.Roles)
            {
                if (role.RoleScope == RoleScope.Merchant)
                {
                    throw new AlreadyInUseException(_localizer.GetString("AlreadyInUseException"));
                }
            }
        }

        await _merchantUserHttpClient.SaveAsync(request);
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
        if (request.PhoneCode is null)
        {
            throw new InvalidParameterException(request.PhoneCode);
        }
        if (request.MobilePhoneNumber is null)
        {
            throw new InvalidParameterException(request.MobilePhoneNumber);
        }
        
        var username = await _userNameGenerator.GetUserName(UserTypePrefix.Corporate,
                    request.PhoneCode, request.MobilePhoneNumber);

        if (request.UserId == Guid.Empty)
        {
            var user = await _userHttpClient.CreateUserAsync(new CreateUserWithUserName
            {
                Email = request.Email,
                FirstName = request.Name,
                LastName = request.Surname,
                BirthDate = request.BirthDate,
                PhoneNumber = request.MobilePhoneNumber,
                Roles = new List<Guid> { Guid.Parse(request.RoleId) },
                UserType = UserType.Corporate,
                PhoneCode = request.PhoneCode,
                UserName = username
            });
            request.UserId = user.UserId;
        }
        else
        {
            await _userHttpClient.UpdateUserAsync(new UpdateUserWithUserName
            {
                RecordStatus = request.RecordStatus,
                Email = request.Email,
                FirstName = request.Name,
                LastName = request.Surname,
                BirthDate = request.BirthDate,
                PhoneCode = request.PhoneCode,
                PhoneNumber = request.MobilePhoneNumber,
                Roles = new List<Guid> { Guid.Parse(request.RoleId) },
                UserType = UserType.Corporate,
                Id = request.UserId,
                UserName = username
            });
        }

        await _merchantUserHttpClient.UpdateAsync(request);
    }
}

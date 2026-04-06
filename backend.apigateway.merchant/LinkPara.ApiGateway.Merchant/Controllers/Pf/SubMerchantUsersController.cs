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
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Authorization;

namespace LinkPara.ApiGateway.Merchant.Controllers.Pf;

public class SubMerchantUsersController : ApiControllerBase
{
    private readonly ISubMerchantUserHttpClient _subMerchantUserHttpClient;
    private readonly IUserHttpClient _userHttpClient;
    private readonly IStringLocalizer _localizer;

    public SubMerchantUsersController(ISubMerchantUserHttpClient subMerchantUserHttpClient,
        IUserHttpClient userHttpClient,
        IStringLocalizerFactory factory)
    {
        _subMerchantUserHttpClient = subMerchantUserHttpClient;
        _userHttpClient = userHttpClient;
        _localizer = factory.Create("Exceptions", "LinkPara.ApiGateway.Merchant");
    }

    /// <summary>
    /// Returns submerchant users
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "SubMerchantUser:ReadAll")]
    public async Task<ActionResult<PaginatedList<SubMerchantUserDto>>> GetAllAsync([FromQuery] GetAllSubMerchantUserRequest request)
    {
        return await _subMerchantUserHttpClient.GetAllAsync(request);
    }

    /// <summary>
    /// Returns a submerchant user
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "SubMerchantUser:Read")]
    public async Task<ActionResult<SubMerchantUserDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await _subMerchantUserHttpClient.GetByIdAsync(id);
    }

    /// <summary>
    /// Create a submerchant user
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    /// <exception cref="AlreadyInUseException"></exception>
    [HttpPost("")]
    [Authorize(Policy = "SubMerchantUser:Create")]
    public async Task SaveAsync(SaveSubMerchantUserRequest request)
    {
        var userPhone = await _userHttpClient.GetAllUsersAsync(new GetUsersRequest
        {
            PhoneNumber = request.MobilePhoneNumber,
            UserStatus = UserStatus.Active,
            UserType = UserType.CorporateSubMerchant
        });

        foreach (var item in userPhone.Items)
        {
            foreach (var role in item.Roles)
            {
                if (role.RoleScope == RoleScope.CorporateSubMerchant)
                {
                    throw new AlreadyInUseException(_localizer.GetString("AlreadyInUseException"));
                }
            }
        }

        var userEmail = await _userHttpClient.GetAllUsersAsync(new GetUsersRequest
        {
            Email = request.Email,
            UserStatus = UserStatus.Active,
            UserType = UserType.CorporateSubMerchant
        });

        foreach (var item in userEmail.Items)
        {
            foreach (var role in item.Roles)
            {
                if (role.RoleScope == RoleScope.CorporateSubMerchant)
                {
                    throw new AlreadyInUseException(_localizer.GetString("AlreadyInUseException"));
                }
            }
        }

        await _subMerchantUserHttpClient.SaveAsync(request);
    }

    /// <summary>
    /// Update a submerchant user
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("")]
    [Authorize(Policy = "SubMerchantUser:Update")]
    public async Task UpdateAsync(SubMerchantUserDto request)
    {
        if (request.PhoneCode is null)
        {
            throw new InvalidParameterException(request.PhoneCode);
        }
        if (request.MobilePhoneNumber is null)
        {
            throw new InvalidParameterException(request.MobilePhoneNumber);
        }
        if (request.IdentityNumber is null)
        {
            throw new InvalidParameterException(request.IdentityNumber);
        }
        
        await _subMerchantUserHttpClient.UpdateAsync(request);
    }
}

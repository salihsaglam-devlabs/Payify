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
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Authorization;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Pf;

public class SubMerchantUsersController : ApiControllerBase
{
    private readonly ISubMerchantUserHttpClient _subMerchantUserHttpClient;
    public SubMerchantUsersController(ISubMerchantUserHttpClient subMerchantUserHttpClient)
    {
        _subMerchantUserHttpClient = subMerchantUserHttpClient;

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
        await _subMerchantUserHttpClient.UpdateAsync(request);
    }
}

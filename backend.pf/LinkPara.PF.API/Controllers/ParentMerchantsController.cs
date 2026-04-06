using LinkPara.PF.Application.Commons.Models.MainSubMerchants;
using LinkPara.PF.Application.Features.Merchants;
using LinkPara.PF.Application.Features.ParentMerchants;
using LinkPara.PF.Application.Features.ParentMerchants.Command.BulkIntegrationModeUpdate;
using LinkPara.PF.Application.Features.ParentMerchants.Command.BulkPermissionUpdate;
using LinkPara.PF.Application.Features.ParentMerchants.Command.BulkPricingProfileUpdate;
using LinkPara.PF.Application.Features.ParentMerchants.Command.UpdateMultipleIntegrationMode;
using LinkPara.PF.Application.Features.ParentMerchants.Command.UpdateMultiplePermission;
using LinkPara.PF.Application.Features.ParentMerchants.Command.UpdateMultiplePricingProfile;
using LinkPara.PF.Application.Features.ParentMerchants.Queries.GetAllParentMerchant;
using LinkPara.PF.Application.Features.ParentMerchants.Queries.GetAllParentMerchantWithPermissions;
using LinkPara.PF.Application.Features.ParentMerchants.Queries.GetParentMerchantById;
using LinkPara.PF.Application.Features.SubMerchants.Command.UpdateMultipleSubMerchant;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.PF.API.Controllers;

public class ParentMerchantsController : ApiControllerBase
{
    /// <summary>
    /// Returns filtered parent merchants
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "ParentMerchant:ReadAll")]
    [HttpGet("")]
    public async Task<ActionResult<PaginatedList<MerchantDto>>> GetFilterAsync([FromQuery] GetAllParentMerchantQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Returns a parent merchant permissions
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "ParentMerchant:ReadAll")]
    [HttpGet("permissions")]
    public async Task<ActionResult<PaginatedList<ParentMerchantDto>>> GetPermissionsAsync([FromQuery] GetAllParentMerchantWithPermissionsQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Returns a parent merchant
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "ParentMerchant:Read")]
    [HttpGet("{id}")]
    public async Task<ActionResult<MerchantDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetParentMerchantByIdQuery { Id = id });
    }

    /// <summary>
    /// Bulk update permissions for sub merchants.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "ParentMerchant:Update")]
    [HttpPut("bulkUpdatePermissions")]
    public async Task BulkUpdatePermissionAsync(BulkPermissionUpdateCommand request)
    {
        await Mediator.Send(request);
    }

    /// <summary>
    /// Bulk update integration mode for sub merchants.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "ParentMerchant:Update")]
    [HttpPut("bulkUpdateIntegrationModes")]
    public async Task BulkUpdateIntegrationModeAsync(BulkIntegrationModeUpdateCommand request)
    {
        await Mediator.Send(request);
    }

    /// <summary>
    /// Bulk update pricing profile for sub merchants.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "ParentMerchant:Update")]
    [HttpPut("bulkUpdatePricingProfiles")]
    public async Task BulkUpdatePricingProfileAsync(BulkPricingProfileUpdateCommand request)
    {
        await Mediator.Send(request);
    }

    /// <summary>
    /// Updates multiple integration mode for sub merchants.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "ParentMerchant:Update")]
    [HttpPut("multipleIntegrationMode")]
    public async Task UpdateMultipleAsync([FromBody] List<UpdateMultipleIntegrationModeModel> request)
    {
        await Mediator.Send(new UpdateMultipleIntegrationModeCommand { MultipleIntegrationModeModel = request});
    }

    /// <summary>
    /// Updates multiple permission for sub merchant.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "ParentMerchant:Update")]
    [HttpPut("multiplePermission")]
    public async Task UpdateMultipleAsync([FromBody] List<UpdateMultiplePermissionModel> request)
    {
        await Mediator.Send(new UpdateMultiplePermissionCommand { MultiplePermissionModel = request});
    }

    /// <summary>
    /// Updates multiple pricing profile for sub merchant.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "ParentMerchant:Update")]
    [HttpPut("multiplePricingProfile")]
    public async Task UpdateMultipleAsync(UpdateMultiplePricingProfileCommand command)
    {
        await Mediator.Send(command);
    }
}

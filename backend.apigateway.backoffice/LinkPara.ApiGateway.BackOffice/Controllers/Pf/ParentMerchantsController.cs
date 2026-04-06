using LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Pf;

public class ParentMerchantsController : ApiControllerBase
{
    private readonly IParentMerchantHttpClient _parentMerchantHttpClient;
    public ParentMerchantsController(IParentMerchantHttpClient parentMerchantHttpClient)
    {
        _parentMerchantHttpClient = parentMerchantHttpClient;
    }

    /// <summary>
    /// Returns filtered parent merchants
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "ParentMerchant:ReadAll")]
    [HttpGet("")]
    public async Task<ActionResult<PaginatedList<MerchantDto>>> GetFilterAsync([FromQuery] GetAllParentMerchantRequest request)
    {
        return await _parentMerchantHttpClient.GetFilterListAsync(request);
    }

    /// <summary>
    /// Returns a parent merchant permissions
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "ParentMerchant:ReadAll")]
    [HttpGet("permissions")]
    public async Task<ActionResult<PaginatedList<ParentMerchantResponse>>> GetPermissionsAsync([FromQuery] GetParentMerchantPermissionsRequest request)
    {
        return await _parentMerchantHttpClient.GetPermissionsAsync(request);
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
        return await _parentMerchantHttpClient.GetByIdAsync(id);
    }

    /// <summary>
    /// Updates multiple integration mode for sub merchants.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "ParentMerchant:Update")]
    [HttpPut("multipleIntegrationMode")]
    public async Task UpdateMultipleAsync(List<UpdateMultipleIntegrationModeRequest> request)
    {
        await _parentMerchantHttpClient.UpdateMultipleIntegrationModeAsync(request);
    }

    /// <summary>
    /// Updates multiple permission for sub merchants.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "ParentMerchant:Update")]
    [HttpPut("multiplePermission")]
    public async Task UpdateMultipleAsync(List<UpdateMultiplePermissionRequest> request)
    {
        await _parentMerchantHttpClient.UpdateMultiplePermissionAsync(request);
    }

    /// <summary>
    /// Updates multiple pricing profile for sub merchants.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "ParentMerchant:Update")]
    [HttpPut("multiplePricingProfile")]
    public async Task UpdateMultipleAsync(UpdateMultiplePricingProfileRequest request)
    {
        await _parentMerchantHttpClient.UpdateMultiplePricingProfileAsync(request);
    }

    /// <summary>
    /// Bulk update permissions for sub merchants.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "ParentMerchant:Update")]
    [HttpPut("bulkUpdatePermissions")]
    public async Task BulkUpdatePermissionAsync(BulkPermissionUpdateRequest request)
    {
        await _parentMerchantHttpClient.BulkUpdatePermissionAsync(request);
    }

    /// <summary>
    /// Bulk update integration mode for sub merchants.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "ParentMerchant:Update")]
    [HttpPut("bulkUpdateIntegrationModes")]
    public async Task BulkUpdateIntegrationModeAsync(BulkIntegrationModeUpdateRequest request)
    {
        await _parentMerchantHttpClient.BulkUpdateIntegrationModeAsync(request);
    }

    /// <summary>
    /// Bulk update pricing profile for sub merchants.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "ParentMerchant:Update")]
    [HttpPut("bulkUpdatePricingProfiles")]
    public async Task BulkUpdatePricingProfileAsync(BulkPricingProfileRequest request)
    {
        await _parentMerchantHttpClient.BulkUpdatePricingProfileAsync(request);
    }
}

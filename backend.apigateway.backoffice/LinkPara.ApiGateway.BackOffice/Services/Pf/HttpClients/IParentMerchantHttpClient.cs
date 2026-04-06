using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public interface IParentMerchantHttpClient
{
    Task<MerchantDto> GetByIdAsync(Guid id);
    Task<PaginatedList<MerchantDto>> GetFilterListAsync(GetAllParentMerchantRequest request);
    Task<PaginatedList<ParentMerchantResponse>> GetPermissionsAsync(GetParentMerchantPermissionsRequest request);
    Task UpdateMultipleIntegrationModeAsync(List<UpdateMultipleIntegrationModeRequest> request);
    Task UpdateMultiplePricingProfileAsync(UpdateMultiplePricingProfileRequest request);
    Task UpdateMultiplePermissionAsync(List<UpdateMultiplePermissionRequest> request);
    Task BulkUpdatePermissionAsync(BulkPermissionUpdateRequest request);
    Task BulkUpdateIntegrationModeAsync(BulkIntegrationModeUpdateRequest request);
    Task BulkUpdatePricingProfileAsync(BulkPricingProfileRequest request);
}

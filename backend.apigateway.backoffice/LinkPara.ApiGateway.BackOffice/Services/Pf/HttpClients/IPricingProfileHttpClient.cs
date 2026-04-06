using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.JsonPatch;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public interface IPricingProfileHttpClient
{
    Task<PricingProfileDto> GetByIdAsync(Guid id);
    Task<PaginatedList<PricingProfileDto>> GetFilterListAsync(GetFilterPricingProfileRequest request);
    Task SaveAsync(SavePricingProfileRequest request);
    Task UpdateAsync(UpdatePricingProfileRequest request);
    Task DeleteProfileAsync(Guid id);
    Task<UpdatePricingProfileRequest> PatchAsync(Guid id, JsonPatchDocument<UpdatePricingProfileRequest> acquireBank);
    Task<PricingProfilePreviewResponse> PreviewPricingProfileUpdateAsync(UpdatePreviewPricingProfileRequest request);
}

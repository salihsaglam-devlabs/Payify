using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.JsonPatch;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public interface ICostProfileHttpClient
{
    Task<CostProfilesDto> GetByIdAsync(Guid id);
    Task<PaginatedList<CostProfilesDto>> GetFilterListAsync(GetFilterCostProfileRequest request);
    Task SaveAsync(SaveCostProfileRequest request);
    Task<UpdateCostProfileRequest> PatchAsync(Guid id, JsonPatchDocument<UpdateCostProfileRequest> costProfilePatch);
    Task<CostProfilePreviewResponse> PreviewCostProfileUpdateAsync(UpdatePreviewCostProfileRequest request);
}

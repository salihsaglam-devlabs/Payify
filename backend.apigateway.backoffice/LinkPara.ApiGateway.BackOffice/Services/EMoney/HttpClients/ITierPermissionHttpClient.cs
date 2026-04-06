using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;
using Microsoft.AspNetCore.JsonPatch;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;

public interface ITierPermissionHttpClient
{
    Task<List<TierPermissionResponse>> GetTierPermissionsAsync(GetTierPermissionsRequest request);
    Task<TierPermissionResponse> GetTierPermissionByIdAsync(Guid id);
    Task CreateTierPermissionAsync(CreateTierPermissionRequest request);
    Task PutTierPermissionAsync(UpdateTierPermissionRequest request);
    Task PatchTierPermissionAsync(Guid id, JsonPatchDocument<TierPermissionResponse> request);
    Task DisableTierPermissionAsync(Guid id);
}